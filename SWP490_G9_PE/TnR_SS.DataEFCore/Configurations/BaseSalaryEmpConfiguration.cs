﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TnR_SS.Domain.Entities;

namespace TnR_SS.DataEFCore.Configurations
{
    public class BaseSalaryEmpConfiguration
    {
        public BaseSalaryEmpConfiguration(EntityTypeBuilder<BaseSalaryEmp> entity)
        {
            entity.ToTable("BaseSalaryEmp");

            entity.Property(e => e.ID).HasColumnName("ID");

            entity.Property(e => e.EmpId)
                .IsRequired();

            entity.HasOne(p => p.Employee)
                .WithMany(b => b.BaseSalaryEmps)
                .HasForeignKey(p => p.EmpId)
                .HasConstraintName("FK_BaseSalaryEmp_Employee");

            entity.Property(e => e.StartDate)
                .IsRequired()
                .HasColumnType("datetime");

            entity.Property(e => e.EndDate)
                .IsRequired(false)
                .HasColumnType("datetime");

            entity.Property(e => e.Salary)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime");

            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime");
        }
    }
}
