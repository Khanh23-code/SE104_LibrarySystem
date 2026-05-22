using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using THUVIENZ.DAL;
using THUVIENZ.Models;

namespace THUVIENZ.BLL
{
    public class FavoriteService
    {
        // Event raised when a favorite is added or removed.
        // Parameters: maDocGia, maSach, added(true=added,false=removed)
        public static event Action<int, int, bool>? FavoriteChanged;
        public async Task<bool> AddFavoriteAsync(int maDocGia, int maSach)
        {
            using var context = new LmsDbContext();
            var existing = await context.SachYeuThichs.FirstOrDefaultAsync(f => f.MaDocGia == maDocGia && f.MaSach == maSach);
            if (existing != null) return false;
            context.SachYeuThichs.Add(new SachYeuThich { MaDocGia = maDocGia, MaSach = maSach });
            await context.SaveChangesAsync();
            try {
                System.Diagnostics.Debug.WriteLine($"FavoriteService: Added favorite {maDocGia}/{maSach}");
                FavoriteChanged?.Invoke(maDocGia, maSach, true);
            } catch { }
            return true;
        }

        public async Task<bool> RemoveFavoriteAsync(int maDocGia, int maSach)
        {
            using var context = new LmsDbContext();
            var existing = await context.SachYeuThichs.FirstOrDefaultAsync(f => f.MaDocGia == maDocGia && f.MaSach == maSach);
            if (existing == null) return false;
            context.SachYeuThichs.Remove(existing);
            await context.SaveChangesAsync();
            try {
                System.Diagnostics.Debug.WriteLine($"FavoriteService: Removed favorite {maDocGia}/{maSach}");
                FavoriteChanged?.Invoke(maDocGia, maSach, false);
            } catch { }
            return true;
        }
    }
}
