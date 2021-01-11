using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiteDB;

namespace Mabron.DiscordBots.Shop
{
    public class ChatRoom : IEquatable<ChatRoom?>
    {
        public ulong Id { get; set; }

        public bool Active { get; set; }

        public List<string> Ignores { get; set; } = new List<string>();

        public List<string> Required { get; set; } = new List<string>();

        public List<string> WishList { get; set; } = new List<string>();

        public float DiscountLimit { get; set; } = 0.3f;

        public override bool Equals(object? obj)
        {
            return Equals(obj as ChatRoom);
        }

        public bool Equals(ChatRoom? other)
        {
            return other != null &&
                   Id == other.Id &&
                   Active == other.Active &&
                   Enumerable.SequenceEqual(Ignores, other.Ignores) &&
                   Enumerable.SequenceEqual(Required, other.Required) &&
                   Enumerable.SequenceEqual(WishList, other.WishList) &&
                   DiscountLimit == other.DiscountLimit;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id,
                Ignores.Aggregate(new HashCode(), (h, x) => { h.Add(x); return h; }, x => x.ToHashCode()),
                Required.Aggregate(new HashCode(), (h, x) => { h.Add(x); return h; }, x => x.ToHashCode()),
                WishList.Aggregate(new HashCode(), (h, x) => { h.Add(x); return h; }, x => x.ToHashCode()),
                DiscountLimit.GetHashCode()
            );
        }

        public static bool operator ==(ChatRoom? left, ChatRoom? right)
        {
            return EqualityComparer<ChatRoom>.Default.Equals(left, right);
        }

        public static bool operator !=(ChatRoom? left, ChatRoom? right)
        {
            return !(left == right);
        }
    }
}
