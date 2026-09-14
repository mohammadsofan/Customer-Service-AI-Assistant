import { Outlet, Link } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';
import { Bot, LogOut, Shield, Sparkles, User as UserIcon } from 'lucide-react';

export const EmployeeLayout = () => {
  const { logout, user, isAdmin } = useAuth();

  return (
    <div dir="rtl" className="min-h-screen bg-slate-50 font-sans flex flex-col">
      {/* Top Navbar */}
      <header className="bg-white/90 backdrop-blur-md sticky top-0 z-30 border-b border-slate-200/80 shadow-xs">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between h-18 items-center">
            {/* Brand */}
            <div className="flex items-center gap-3">
              <div className="w-11 h-11 rounded-xl bg-white border border-slate-200/80 shadow-sm p-1.5 flex items-center justify-center shrink-0">
                <img src="/brand/emblem.png" alt="Emblem" className="w-full h-full object-contain rounded-lg" />
              </div>
              <div>
                <div className="flex items-center gap-2">
                  <h1 className="text-base font-bold text-slate-900 tracking-tight">
                    مساعد خدمة العملاء
                  </h1>
                  <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full bg-[#76bc21]/15 border border-[#76bc21]/30 text-[11px] font-semibold text-[#3a690d]">
                    <Sparkles className="w-3 h-3 text-[#76bc21]" />
                    <span>الذكاء الاصطناعي</span>
                  </span>
                </div>
                <p className="text-xs text-slate-400">
                  بوابة دعم الموظفين والرد المعرفي الفوري
                </p>
              </div>
            </div>

            {/* Right Controls */}
            <div className="flex items-center gap-3">
              {isAdmin && (
                <Link
                  to="/admin"
                  className="hidden sm:inline-flex items-center gap-1.5 px-3.5 py-2 rounded-xl bg-slate-100 hover:bg-slate-200 text-slate-700 text-xs font-semibold transition-colors"
                >
                  <Shield className="w-3.5 h-3.5 text-[#0055b8]" />
                  <span>لوحة الإدارة</span>
                </Link>
              )}

              {/* User Chip */}
              <div className="flex items-center gap-2.5 px-3 py-1.5 rounded-xl bg-slate-50 border border-slate-200/70">
                <div className="w-7 h-7 rounded-lg bg-[#76bc21]/15 text-[#3d6e0e] flex items-center justify-center font-bold text-xs">
                  <UserIcon className="w-4 h-4" />
                </div>
                <div className="text-right">
                  <div className="text-xs font-bold text-slate-800">
                    {user?.fullName || user?.username || 'الموظف'}
                  </div>
                  <div className="text-[10px] text-slate-400">
                    {user?.role || 'دعم فني'}
                  </div>
                </div>
              </div>

              {/* Logout Button */}
              <button
                onClick={logout}
                title="تسجيل الخروج"
                className="p-2.5 rounded-xl text-slate-500 hover:text-rose-600 hover:bg-rose-50 border border-transparent hover:border-rose-100 transition-all cursor-pointer"
              >
                <LogOut className="w-4 h-4" />
              </button>
            </div>
          </div>
        </div>
      </header>

      {/* Main Content Viewport */}
      <main className="flex-1 max-w-7xl w-full mx-auto p-4 sm:p-6 lg:p-8">
        <Outlet />
      </main>

      {/* Footer */}
      <footer className="py-4 border-t border-slate-200/60 text-center text-xs text-slate-400">
        نظام دعم الموظفين الذكي &copy; {new Date().getFullYear()} — المساعد الذكي لموظفي خدمة العملاء والدعم الفني
      </footer>
    </div>
  );
};
