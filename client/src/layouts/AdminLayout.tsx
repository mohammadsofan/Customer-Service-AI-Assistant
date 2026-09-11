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
  LogOut
} from 'lucide-react';

const sidebarItems = [
  { name: 'الرئيسية', path: '/admin', icon: LayoutDashboard },
  { name: 'قاعدة المعرفة', path: '/admin/knowledge', icon: BookOpen },
  { name: 'الذكاء الاصطناعي', path: '/admin/ai', icon: Bot },
  { name: 'الموظفون', path: '/admin/employees', icon: Users },
  { name: 'الأسئلة', path: '/admin/questions', icon: MessageSquare },
  { name: 'الإحصائيات', path: '/admin/analytics', icon: BarChart2 },
  { name: 'سجل التدقيق', path: '/admin/audit', icon: FileText },
];

export const AdminLayout = () => {
  const { logout, user } = useAuth();
  const location = useLocation();

  return (
    <div dir="rtl" className="flex h-screen bg-gray-100 font-sans">
      {/* Sidebar */}
      <div className="w-64 bg-white border-l border-gray-200 flex flex-col">
        <div className="h-16 flex items-center justify-center border-b border-gray-200">
          <h1 className="text-xl font-bold text-blue-600">لوحة الإدارة</h1>
        </div>
        <nav className="flex-1 overflow-y-auto py-4">
          <ul className="space-y-1 px-2">
            {sidebarItems.map((item) => {
              const Icon = item.icon;
              const isActive = location.pathname === item.path;
              return (
                <li key={item.path}>
                  <Link
                    to={item.path}
                    className={`flex items-center px-4 py-2.5 text-sm font-medium rounded-md transition-colors ${
                      isActive
                        ? 'bg-blue-50 text-blue-700'
                        : 'text-gray-700 hover:bg-gray-50 hover:text-gray-900'
                    }`}
                  >
                    <Icon className="ml-3 h-5 w-5 flex-shrink-0" />
                    {item.name}
                  </Link>
                </li>
              );
            })}
          </ul>
        </nav>
      </div>

      {/* Main Content */}
      <div className="flex-1 flex flex-col overflow-hidden">
        {/* Header */}
        <header className="h-16 bg-white border-b border-gray-200 flex items-center justify-between px-6">
          <div className="text-xl font-semibold text-gray-800">
            {sidebarItems.find(item => item.path === location.pathname)?.name || 'لوحة الإدارة'}
          </div>
          <div className="flex items-center space-x-4 space-x-reverse">
            <span className="text-sm text-gray-700">{user?.username || 'المدير'}</span>
            <button
              onClick={logout}
              className="flex items-center text-sm text-red-600 hover:text-red-800 transition-colors"
            >
              <LogOut className="ml-1.5 h-4 w-4" />
              خروج
            </button>
          </div>
        </header>

        {/* Content */}
        <main className="flex-1 overflow-y-auto p-6 bg-gray-50">
          <Outlet />
        </main>
      </div>
    </div>
  );
};
