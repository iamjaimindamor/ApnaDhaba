using Microsoft.EntityFrameworkCore;

namespace ApnaDhaba.Api_Models;

public partial class ApnaDbContext : DbContext
{
    public ApnaDbContext()
    {
    }

    public ApnaDbContext(DbContextOptions<ApnaDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AssignedRole> AssignedRoles { get; set; }

    public virtual DbSet<CategoryTable> CategoryTables { get; set; }

    public virtual DbSet<ProductTable> ProductTables { get; set; }

    public virtual DbSet<RoleModel> RoleModels { get; set; }

    public virtual DbSet<UserModel> UserModels { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AssignedRole>(entity =>
        {
            entity.HasKey(e => e.SerialId);

            entity.ToTable("assignedRoles");

            entity.Property(e => e.SerialId).HasColumnName("serialID");
            entity.Property(e => e.RoleId).HasColumnName("roleId");
            entity.Property(e => e.UserId).HasColumnName("userId");

            entity.HasOne(d => d.Role).WithMany(p => p.AssignedRoles)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK_assignedRoles_roleModels_roleId1");

            entity.HasOne(d => d.User).WithMany(p => p.AssignedRoles)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_assignedRoles_userModels_userId1");
        });

        modelBuilder.Entity<CategoryTable>(entity =>
        {
            entity.HasKey(e => e.CategoryId);

            entity.ToTable("categoryTable");
        });

        modelBuilder.Entity<ProductTable>(entity =>
        {
            entity.HasKey(e => e.ProductId);

            entity.ToTable("productTable");

            entity.HasIndex(e => e.CategoryId1, "IX_productTable_CategoryId1");

            entity.Property(e => e.ImageUrl).HasColumnName("ImageURL");

            entity.HasOne(d => d.CategoryId1Navigation).WithMany(p => p.ProductTables).HasForeignKey(d => d.CategoryId1);
        });

        modelBuilder.Entity<RoleModel>(entity =>
        {
            entity.HasKey(e => e.RoleId);

            entity.ToTable("roleModels");

            entity.Property(e => e.RoleId).HasColumnName("roleId");
            entity.Property(e => e.RoleName).HasColumnName("roleName");
        });

        modelBuilder.Entity<UserModel>(entity =>
        {
            entity.HasKey(e => e.userId);

            entity.ToTable("userModels");

            entity.Property(e => e.userId).HasColumnName("userId");
            entity.Property(e => e.ImageURL).HasColumnName("ImageURL");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}