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
    public class HistorySalaryEmpConfiguration
    {
        public HistorySalaryEmpConfiguration(EntityTypeBuilder<HistorySalaryEmp> entity)
        {
            entity.ToTable("HistorySalaryEmp");

            entity.Property(e => e.ID).HasColumnName("ID");

            entity.Property(e => e.EmpId)
                .IsRequired();

            entity.HasOne(p => p.Employee)
                .WithMany(b => b.HistorySalaryEmps)
                .HasForeignKey(p => p.EmpId)
                .HasConstraintName("FK_HistorySalaryEmp_Employee");

            // entity.Property(e => e.Month)
            //     .IsRequired();

            // entity.Property(e => e.Year)
            //     .IsRequired();

            entity.Property(e => e.DateStart).IsRequired();
            entity.Property(e => e.DateEnd);

            entity.Property(e => e.Salary)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime");

            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime");

            entity.Property(e => e.Bonus);

            entity.Property(e => e.Punish);
        }
    }
}
