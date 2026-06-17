using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using OutfitShop.Interfaces;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using System.IO;

namespace OutfitShop.Controllers
{
    [Authorize] // ← Toàn bộ controller yêu cầu đăng nhập
    public class PhieuPhoiDoController : Controller
    {
        private readonly IProductService _productService;
        private readonly IWebHostEnvironment _env;

        private const string API_BASE = "https://platform.fitroom.app";
        private const string API_KEY = "b4c51bebad12456eb8edb39b91c64e2cd65ef8d7efef0fa0296cb4f3d4b0d5ef";

        public PhieuPhoiDoController(IProductService productService, IWebHostEnvironment env)
        {
            _productService = productService;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllProductsAsync();
            return View(products);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessAITryOn(IFormFile userImage, string garmentImageUrl, string clothType = "upper")
        {
            try
            {
                // ── Bước 1: Đọc ảnh người dùng upload lên ──────────────────
                byte[] userImageBytes;
                using (var ms = new MemoryStream())
                {
                    await userImage.CopyToAsync(ms);
                    userImageBytes = ms.ToArray();
                }

                // ── Bước 2: Lấy ảnh trang phục (URL ngoài hoặc file local) ─
                byte[] garmentImageBytes;

                if (garmentImageUrl.StartsWith("http://") || garmentImageUrl.StartsWith("https://"))
                {
                    using var httpDownload = new HttpClient();
                    httpDownload.Timeout = TimeSpan.FromSeconds(15);
                    garmentImageBytes = await httpDownload.GetByteArrayAsync(garmentImageUrl);
                }
                else
                {
                    string garmentPath = Path.Combine(_env.WebRootPath, garmentImageUrl.TrimStart('/'));
                    if (!System.IO.File.Exists(garmentPath))
                        return Json(new { success = false, message = "Không tìm thấy ảnh trang phục trên server." });
                    garmentImageBytes = await System.IO.File.ReadAllBytesAsync(garmentPath);
                }

                // ── Bước 3: Gửi lên FitRoom API ────────────────────────────
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(120);
                client.DefaultRequestHeaders.Add("X-API-KEY", API_KEY);

                string taskId;
                using (var formContent = new MultipartFormDataContent())
                {
                    var modelContent = new ByteArrayContent(userImageBytes);
                    modelContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
                    formContent.Add(modelContent, "model_image", "model.jpg");

                    var clothContent = new ByteArrayContent(garmentImageBytes);
                    clothContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
                    formContent.Add(clothContent, "cloth_image", "cloth.jpg");

                    formContent.Add(new StringContent(clothType), "cloth_type");

                    var response = await client.PostAsync($"{API_BASE}/api/tryon/v2/tasks", formContent);
                    var responseStr = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                        return Json(new { success = false, message = $"FitRoom từ chối ({(int)response.StatusCode}): {responseStr}" });

                    using var doc = JsonDocument.Parse(responseStr);
                    if (!doc.RootElement.TryGetProperty("task_id", out var taskEl))
                        return Json(new { success = false, message = "Không nhận được task_id: " + responseStr });

                    taskId = taskEl.GetString()!;
                }

                // ── Bước 4: Polling kết quả ─────────────────────────────────
                string pollUrl = $"{API_BASE}/api/tryon/v2/tasks/{taskId}";

                for (int i = 0; i < 20; i++)
                {
                    await Task.Delay(3000);

                    var pollResponse = await client.GetAsync(pollUrl);
                    var pollStr = await pollResponse.Content.ReadAsStringAsync();

                    using var pollDoc = JsonDocument.Parse(pollStr);
                    var pollRoot = pollDoc.RootElement;
                    string status = pollRoot.GetProperty("status").GetString()!;

                    if (status == "COMPLETED")
                    {
                        string resultUrl = pollRoot.GetProperty("download_signed_url").GetString()!;
                        return Json(new { success = true, resultImage = resultUrl, isFallback = false });
                    }

                    if (status == "FAILED")
                    {
                        string errMsg = pollRoot.TryGetProperty("error", out var errEl)
                            ? errEl.GetString()!
                            : "Lỗi không xác định từ FitRoom.";
                        return Json(new { success = false, message = "FitRoom xử lý thất bại: " + errMsg });
                    }
                }

                return Json(new { success = false, message = "Timeout: FitRoom xử lý quá 60 giây. Vui lòng thử lại." });
            }
            catch (TaskCanceledException)
            {
                return Json(new { success = false, message = "Kết nối tới FitRoom bị timeout. Thử lại sau." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }
    }
}