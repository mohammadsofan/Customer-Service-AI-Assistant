import { Outlet, Link, useLocation } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';
import { 
  LayoutDashboard, 
  BookOpen, 
  Bot, 
  Users, 
  MessageSquare, 
  BarChart2, 
  FileText,
  LogOut,
  Sliders,
  Cpu,
  Layers,
  Sparkles
} from 'lucide-react';

const navGroups = [
  {
    title: 'نظرة عامة',
    items: [
      { name: 'لوحة التحكم', path: '/admin', icon: LayoutDashboard },
      { name: 'الأسئلة والاستفسارات', path: '/admin/questions', icon: MessageSquare },
      { name: 'التحليلات والتقارير', path: '/admin/analytics', icon: BarChart2 },
    ]
  },
  {
    title: 'إدارة المعرفة',
    items: [
      { name: 'سيناريوهات الدعم', path: '/admin/knowledge', icon: BookOpen },
      { name: 'التصنيفات', path: '/admin/knowledge/categories', icon: Layers },
      { name: 'الكلمات المفتاحية', path: '/admin/knowledge/keywords', icon: Sparkles },
    ]
  },
  {
    title: 'الذكاء الاصطناعي',
    items: [
      { name: 'إعدادات النموذج', path: '/admin/ai', icon: Sliders },
      { name: 'مزودو الخدمة', path: '/admin/ai/providers', icon: Cpu },
      { name: 'نماذج الذكاء', path: '/admin/ai/models', icon: Bot },
    ]
  },
  {
    title: 'النظام والموظفون',
    items: [
      { name: 'فريق العمل', path: '/admin/employees', icon: Users },
      { name: 'سجل التدقيق والأمان', path: '/admin/audit', icon: FileText },
    ]
  }
];

export const AdminLayout = () => {
  const { logout, user } = useAuth();
  const location = useLocation();

  const allItems = navGroups.flatMap(g => g.items);
  const currentItem = allItems.find(item => item.path === location.pathname);

  return (
    <div dir="rtl" className="flex h-screen bg-[#f5f5f7] font-sans overflow-hidden">
      {/* Sidebar */}
      <aside className="w-72 bg-[#002654] text-slate-200 flex flex-col shadow-xl z-20 border-l border-[#001c3d]">
        {/* Brand Header */}
        <div className="h-20 px-6 flex items-center gap-3.5 border-b border-[#001c3d] bg-[#001d40]">
          <div className="w-11 h-11 rounded-xl bg-white border border-white/20 shadow-xs p-1.5 flex items-center justify-center shrink-0">
            <img src="/brand/emblem.png" alt="Emblem" className="w-full h-full object-contain rounded-lg" />
          </div>
          <div>
            <h1 className="font-bold text-base text-white tracking-tight flex items-center gap-1.5">
              <span>مساعد الدعم الذكي</span>
            </h1>
            <span className="text-xs text-[#76bc21] font-medium flex items-center gap-1.5">
              <span className="w-1.5 h-1.5 rounded-full bg-[#76bc21]" />
              لوحة الإدارة المركزية
            </span>
          </div>
        </div>

        {/* Navigation */}
        <nav className="flex-1 overflow-y-auto px-4 py-5 space-y-6">
          {navGroups.map((group, idx) => (
            <div key={idx} className="space-y-1.5">
              <div className="px-3 text-[11px] font-bold uppercase tracking-wider text-blue-200/60 mb-2">
                {group.title}
              </div>
              {group.items.map((item) => {
                const Icon = item.icon;
                const isActive = location.pathname === item.path;
                return (
                  <Link
                    key={item.path}
                    to={item.path}
                    className={`flex items-center gap-3 px-3.5 py-2.5 rounded-xl text-sm font-medium transition-all duration-150 ${
                      isActive
                        ? 'bg-[#76bc21] text-white shadow-sm font-semibold'
                        : 'text-slate-200 hover:text-white hover:bg-white/10'
                    }`}
                  >
                    <Icon className={`w-4 h-4 ${isActive ? 'text-white' : 'text-blue-200/70'}`} />
                    <span>{item.name}</span>
                  </Link>
                );
              })}
            </div>
          ))}
        </nav>

        {/* User Card & Logout in Footer */}
        <div className="p-4 border-t border-[#001c3d] bg-[#001d40]">
          <div className="flex items-center justify-between gap-3">
            <div className="flex items-center gap-2.5 overflow-hidden">
              <div className="w-9 h-9 rounded-full bg-[#76bc21]/20 border border-[#76bc21]/40 text-[#76bc21] flex items-center justify-center font-bold text-sm shrink-0">
                {(user?.fullName || user?.email || 'M')[0].toUpperCase()}
              </div>
              <div className="overflow-hidden">
                <div className="text-sm font-semibold text-white truncate">
                  {user?.fullName || user?.username || 'مدير النظام'}
                </div>
                <div className="text-xs text-blue-200/70 truncate dir-ltr text-right">
                  {user?.email}
                </div>
              </div>
            </div>

            <button
              onClick={logout}
              title="تسجيل الخروج"
              className="p-2 rounded-lg text-slate-300 hover:text-rose-300 hover:bg-rose-500/20 transition-colors cursor-pointer"
            >
              <LogOut className="w-5 h-5" />
            </button>
          </div>
        </div>
      </aside>

      {/* Main Container */}
      <div className="flex-1 flex flex-col min-w-0 overflow-hidden">
        {/* Top Header */}
        <header className="h-20 bg-white border-b border-slate-200/80 px-8 flex items-center justify-between shadow-xs z-10">
          <div>
            <h2 className="text-xl font-bold text-slate-900 tracking-tight">
              {currentItem?.name || 'لوحة الإدارة'}
            </h2>
            <p className="text-xs text-slate-500 mt-0.5">
              إدارة العمليات المعرفية والذكاء الاصطناعي بكفاءة
            </p>
          </div>

          <div className="flex items-center gap-4">
            <div className="flex items-center gap-2 px-3.5 py-1.5 rounded-full bg-[#76bc21]/10 border border-[#76bc21]/30 text-xs font-semibold text-[#3b6b0c]">
              <span className="w-2 h-2 rounded-full bg-[#76bc21] animate-pulse" />
              <span>النظام متصل ونشط</span>
            </div>

            <Link
              to="/support"
              className="px-3.5 py-1.5 rounded-xl border border-slate-200 text-xs font-medium text-slate-700 hover:bg-[#76bc21]/5 hover:border-[#76bc21]/60 hover:text-[#4d8112] transition-colors flex items-center gap-1.5"
            >
              <span>بوابة الموظف</span>
            </Link>
          </div>
        </header>

        {/* Page Content Viewport */}
        <main className="flex-1 overflow-y-auto p-8">
          <div className="max-w-7xl mx-auto">
            <Outlet />
          </div>
        </main>
      </div>
    </div>
  );
};
