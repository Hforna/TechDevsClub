using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Profile.Domain.Aggregates;
using Profile.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Domain.Entities
{
    public class Connection : IEntity
    {
        public long Id { get; set; }
        [ForeignKey("ConnectorId")]
        public ProfileEntity Connector { get; set; }
        public long ConnectorId { get; set; }
        [ForeignKey("ConnectedId")]
        public ProfileEntity Connected { get; set; }
        public long ConnectedId { get; set; }
        public bool AreConnected { get; set; }
        public ConnectionStatus Status { get; set; } = ConnectionStatus.Pending;
        public DateTime ConnectedAt { get; private set; } = DateTime.UtcNow;

        public void Accept()
        {
            if (Status != ConnectionStatus.Pending)
                throw new DomainException(ResourceExceptMessages.ONLY_ACCEPT_PENDING_CONNECTION, System.Net.HttpStatusCode.Unauthorized);

            Status = ConnectionStatus.Approved;
        }

        public void Reject() => Status = ConnectionStatus.Rejected;

        public class Mapping : IEntityTypeConfiguration<Connection>
        {
            public void Configure(EntityTypeBuilder<Connection> builder)
            {
                builder.HasKey(x => x.Id);

                builder.HasOne(x => x.Connector)
                    .WithMany()
                    .HasForeignKey(x => x.ConnectorId)
                    .OnDelete(DeleteBehavior.NoAction); 

                builder.HasOne(x => x.Connected)
                    .WithMany()
                    .HasForeignKey(x => x.ConnectedId)
                    .OnDelete(DeleteBehavior.NoAction);

                builder.HasIndex(x => new { x.ConnectorId, x.ConnectedId }).IsUnique();
            }
        }
    }

    public enum ConnectionStatus
    {
        Rejected,
        Pending,
        Approved
    }
}
