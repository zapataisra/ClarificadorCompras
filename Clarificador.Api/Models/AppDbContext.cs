using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace Clarificador.Api.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ConsultasUsuario> ConsultasUsuarios { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<VariacionesProducto> VariacionesProductos { get; set; }

    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_unicode_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<ConsultasUsuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasIndex(e => e.ProductoId, "ProductoId");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.Estado)
                .HasDefaultValueSql("'pendiente'")
                .HasColumnType("enum('pendiente','completado','fallido')");
            entity.Property(e => e.FechaConsulta)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.MotivoFallo).HasMaxLength(255);
            entity.Property(e => e.ProductoId).HasColumnType("int(11)");
            entity.Property(e => e.TelefonoUsuario).HasMaxLength(50);

            entity.HasOne(d => d.Producto).WithMany(p => p.ConsultasUsuarios)
                .HasForeignKey(d => d.ProductoId)
                .HasConstraintName("ConsultasUsuarios_ibfk_1");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasIndex(e => e.UrlNormalizada, "idx_url_normalizada").IsUnique();

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.NombreProducto).HasMaxLength(255);
            entity.Property(e => e.TieneCupon).HasDefaultValueSql("'0'");
            entity.Property(e => e.TieneOfertaRelampago).HasDefaultValueSql("'0'");
            entity.Property(e => e.UltimaActualizacion)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.UltimoPrecioBase).HasPrecision(10, 2);
            entity.Property(e => e.UrlNormalizada).HasMaxLength(500);
            entity.Property(e => e.UrlProducto).HasMaxLength(500);
        });

        modelBuilder.Entity<VariacionesProducto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("VariacionesProducto");

            entity.HasIndex(e => e.ProductoId, "ProductoId");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.NombreVariacion).HasMaxLength(255);
            entity.Property(e => e.PrecioReal).HasPrecision(10, 2);
            entity.Property(e => e.ProductoId).HasColumnType("int(11)");

            entity.HasOne(d => d.Producto).WithMany(p => p.VariacionesProductos)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("VariacionesProducto_ibfk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
