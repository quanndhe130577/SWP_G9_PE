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
    public class PondOwnerConfiguration
    {
        public PondOwnerConfiguration(EntityTypeBuilder<PondOwner> entity)
        {

            entity.ToTable("PondOwner");

            /*entity.HasNoKey();*/
            entity.Property(e => e.ID).HasColumnName("ID");

            entity.Property(e => e.Name)
                .IsRequired();

            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(12)
                .IsRequired()
                .IsUnicode(false);

            entity.Property(e => e.Address)
                .IsRequired();

            entity.HasOne(p => p.Trader)
               .WithMany(b => b.PondOwners)
               .HasForeignKey(p => p.TraderID)
               .OnDelete(DeleteBehavior.ClientNoAction)
               .HasConstraintName("FK_PondOwner_UserInfor");
        }
    }
}
