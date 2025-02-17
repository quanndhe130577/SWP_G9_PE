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
    public class AdvanceSalaryConfiguration
    {
        public AdvanceSalaryConfiguration(EntityTypeBuilder<AdvanceSalary> entity)
        {
            entity.ToTable("AdvanceSalary");

            entity.Property(e => e.ID).HasColumnName("ID");

            entity.Property(e => e.EmpId)
                .IsRequired();

            entity.HasOne(p => p.Employee)
                .WithMany(b => b.AdvanceSalaries)
                .HasForeignKey(p => p.EmpId)
                .HasConstraintName("FK_AdvanceSalary_Employee");

            entity.Property(e => e.Date)
                .IsRequired()
                .HasColumnType("datetime");

            entity.Property(e => e.Amount)
                .IsRequired();
        }
    }
}
