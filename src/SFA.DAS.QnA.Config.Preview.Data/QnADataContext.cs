﻿using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SFA.DAS.QnA.Api.Types;
using SFA.DAS.QnA.Api.Types.Page;

namespace SFA.DAS.QnA.Config.Preview.Data
{
    public class QnaDataContext : DbContext
    {
        public QnaDataContext(DbContextOptions<QnaDataContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           
            modelBuilder.Entity<WorkflowSection>()
                .Property(c => c.QnAData)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v,
                        new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
                    v => JsonConvert.DeserializeObject<QnAData>(v,
                        new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
        }

        public DbSet<Workflow> Workflows { get; set; }
        public DbSet<WorkflowSection> WorkflowSections { get; set; }
        public DbSet<WorkflowSequence> WorkflowSequences { get; set; }
        public DbSet<Project> Projects { get; set; }
    }
}
