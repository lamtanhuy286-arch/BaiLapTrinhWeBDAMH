using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization; // Thêm namespace cho đa ngôn ngữ
using Microsoft.EntityFrameworkCore;
using OutfitShop.Data;
using OutfitShop.Interfaces;
using OutfitShop.Models;
using OutfitShop.Repositories;
using OutfitShop.Services;
using System.Globalization; // Thêm namespace cho CultureInfo

var builder = WebApplication.CreateBuilder(args);
// Đóng gói cấu hình Stripe từ appsettings
Stripe.StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe")["SecretKey"];

// --- CẤU HÌNH DỊCH VỤ DATABASE ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        // Tự động thử lại kết nối khi bị lỗi kết nối tạm thời (Fix lỗi transient failure)
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    }));

// SỬA LỖI: Đổi IdentityUser thành ApplicationUser cho khớp với Database
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// --- CẤU HÌNH ĐĂNG NHẬP BẰNG NGOẠI BIẾN (GOOGLE & FACEBOOK) ---
builder.Services.AddAuthentication()
    .AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
        googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
    })
    .AddFacebook(facebookOptions =>
    {
        facebookOptions.AppId = builder.Configuration["Authentication:Facebook:AppId"]!;
        facebookOptions.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"]!;
    });

// --- ĐĂNG KÝ DEPENDENCY INJECTION (DI) ---
// Bước này bắt buộc phải có để các Controller gọi được Service
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// --- CẤU HÌNH API VÀ SESSION ---
// 1. ĐĂNG KÝ CẤU HÌNH MOMO
builder.Services.Configure<MomoOption>(builder.Configuration.GetSection("MomoAPI"));

// 2. THÊM DỊCH VỤ SESSION (Giỏ hàng)
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// --- [THÊM MỚI] CẤU HÌNH ĐA NGÔN NGỮ (LOCALIZATION) ---
// Chỉ định hệ thống tìm kiếm file dịch tại thư mục "Resources"
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// Tích hợp Localization vào hệ thống MVC (Views & Validation lỗi dữ liệu đầu vào)
builder.Services.AddControllersWithViews()
    .AddViewLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization(options => {
        options.DataAnnotationLocalizerProvider = (type, factory) =>
            factory.Create(typeof(OutfitShop.Resources.SharedResource));
    });

builder.Services.AddRazorPages();

var app = builder.Build();

// --- TẠO TÀI KHOẢN ADMIN MẶC ĐỊNH KHI CHẠY WEB ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        // Gọi hàm SeedAdminAsync để tự động cấp tài khoản Admin@123
        await SeedData.SeedAdminAsync(userManager, roleManager);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Lỗi khi tạo Seed Data: " + ex.Message);
    }
}

// --- CẤU HÌNH MIDDLEWARE ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// 3. KÍCH HOẠT SESSION (Bắt buộc phải đặt trước Authentication)
app.UseSession();

// --- [THÊM MỚI] ĐỊNH NGHĨA MIDDLEWARE XỬ LÝ NGÔN NGỮ ĐẦU VÀO ---
// Thiết lập danh sách các mã ngôn ngữ hệ thống chấp nhận xử lý (vi: Việt, en: Anh)
var supportedCultures = new[] {
    new CultureInfo("vi"),
    new CultureInfo("en")
};

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("vi"), // Mặc định hiển thị là Tiếng Việt
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});
// -------------------------------------------------------------

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();