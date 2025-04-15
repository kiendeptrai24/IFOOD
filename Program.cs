using iFood.Data;
using iFood.Helpers;
using iFood.Interfaces;
using iFood.Models;
using iFood.Models.Momo;
using iFood.Models.ZaloPay;
using iFood.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Repository.Interfaces;

    var builder = WebApplication.CreateBuilder(args);


// cau hinh APA gemini
builder.Services.AddHttpClient();
// Cấu hình Momo API Payment
builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("MomoAPI"));
builder.Services.Configure<ZaloPayOptionModel>(builder.Configuration.GetSection("ZaloPayAPI"));
builder.Services.AddScoped<IMomoService, MomoService>();
builder.Services.AddScoped<IZaloPayService, ZaloPayService>();
builder.Services.AddScoped<PaymentToggle>();


// Thêm các dịch vụ vào container
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IMomoRepository, MomoRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IPhotoService, PhotoService>();



// Cấu hình Cloudinary
builder.Services.Configure<CloudinarySetting>(builder.Configuration.GetSection("CloudinarySettings"));

// Cấu hình database
builder.Services.AddDbContext<ApplicationDBContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Cấu hình Identity
builder.Services.AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDBContext>()
    .AddDefaultTokenProviders(); // Thêm dòng này để kích hoạt Token Providers

// Cấu hình Session
builder.Services.AddDistributedMemoryCache(); 
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(1440); // ⚡ Timeout 30 phút
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

//  Cấu hình Authentication & Cookie
// builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//     .AddCookie();

// cau hinh gg
builder.Services.AddAuthentication(options =>
{
    // options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    // options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    // options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
    //options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;  
})
.AddCookie()
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["GoogleKeys:ClientId"];
    options.ClientSecret = builder.Configuration["GoogleKeys:ClientSecret"];
});


var app = builder.Build();

//  Seed dữ liệu (nếu có)
if (args.Length == 1 && args[0].ToLower() == "seeddata")
{
    Seed.SeedData(app);
}

// Cấu hình pipeline xử lý request
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// QUAN TRỌNG: Kích hoạt Session trước khi Authorization
app.UseSession(); //  

app.UseAuthentication(); //  Đăng nhập
app.UseAuthorization();  //  Xác thực quyền truy cập

// Định tuyến và tĩnh
app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
).WithStaticAssets();

app.Run();
