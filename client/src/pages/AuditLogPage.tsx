import { useEffect, useState, useMemo } from 'react';
import auditService, { AuditLog } from '../services/auditService';
import { DataTable } from '../components/DataTable';
import { SearchBox } from '../components/SearchBox';
import { LoadingState } from '../components/LoadingState';
import { ErrorState } from '../components/ErrorState';

export const AuditLogPage = () => {
  const [logs, setLogs] = useState<AuditLog[]>([]);
  const [initialLoading, setInitialLoading] = useState(true);
  const [isSearching, setIsSearching] = useState(false);
  const [error, setError] = useState<string | null>(null);
  
  const [search, setSearch] = useState('');
  const [debouncedSearch, setDebouncedSearch] = useState('');
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const pageSize = 15;

  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedSearch(search);
      setPage(1);
    }, 400);
    return () => clearTimeout(timer);
  }, [search]);

  useEffect(() => {
    fetchLogs(page, debouncedSearch);
  }, [page, debouncedSearch]);

  const fetchLogs = async (currentPage = page, currentSearch = debouncedSearch) => {
    try {
      setIsSearching(true);
      const data = await auditService.getAuditLogs(currentPage, pageSize, currentSearch);
      setLogs(data.items);
      setTotalPages(data.totalPages);
      setTotalCount(data.totalCount);
      setError(null);
    } catch (err) {
      setError('حدث خطأ أثناء تحميل سجل التدقيق');
    } finally {
      setIsSearching(false);
      setInitialLoading(false);
    }
  };

  const actionLabels: Record<string, { label: string; bg: string; text: string }> = {
    ScenarioCreated: { label: 'إنشاء سيناريو', bg: 'bg-emerald-100', text: 'text-emerald-800' },
    ScenarioUpdated: { label: 'تعديل سيناريو', bg: 'bg-blue-100', text: 'text-blue-800' },
    ScenarioArchived: { label: 'أرشفة سيناريو', bg: 'bg-rose-100', text: 'text-rose-800' },
    StatusChanged: { label: 'تغيير الحالة', bg: 'bg-purple-100', text: 'text-purple-800' },
    CategoryChanged: { label: 'تعديل تصنيف', bg: 'bg-indigo-100', text: 'text-indigo-800' },
    KeywordAdded: { label: 'إضافة كلمة مفتاحية', bg: 'bg-teal-100', text: 'text-teal-800' },
    KeywordRemoved: { label: 'حذف كلمة مفتاحية', bg: 'bg-orange-100', text: 'text-orange-800' },
    AIProviderFailoverTriggered: { label: 'تبديل مزود الذكاء الاصطناعي', bg: 'bg-amber-100', text: 'text-amber-800' },
    AIProviderConfigChanged: { label: 'تعديل إعدادات المزود', bg: 'bg-cyan-100', text: 'text-cyan-800' },
    AIModelChanged: { label: 'تعديل نموذج الذكاء الاصطناعي', bg: 'bg-sky-100', text: 'text-sky-800' },
    UserLoggedIn: { label: 'تسجيل دخول', bg: 'bg-green-100', text: 'text-green-800' }
  };

  if (initialLoading) return <LoadingState />;
  if (error) return <ErrorState message={error} onRetry={() => fetchLogs(1, '')} />;

  const columns = [
    { 
      key: 'user', 
      header: 'المستخدم القائم بالعملية',
      cell: (item: AuditLog) => {
        const isSystem = item.userName.includes('النظام') || item.userId === '00000000-0000-0000-0000-000000000000' || (!item.userEmail && !item.userId);
        return (
          <div className="flex items-center gap-2.5">
            <div className={`w-8 h-8 rounded-full flex items-center justify-center font-bold text-xs shrink-0 ${isSystem ? 'bg-purple-100 text-purple-700' : 'bg-blue-100 text-blue-700'}`}>
              {isSystem ? '⚙️' : (item.userName ? item.userName.charAt(0).toUpperCase() : '؟')}
            </div>
            <div className="flex flex-col min-w-0">
              <div className="flex items-center gap-1.5">
                <span className="font-semibold text-gray-900 text-sm truncate">{item.userName}</span>
                {item.userRole && (
                  <span className="text-[10px] bg-gray-100 text-gray-600 px-1.5 py-0.5 rounded font-medium">
                    {item.userRole}
                  </span>
                )}
              </div>
              {item.userEmail && (
                <span className="text-xs text-gray-400 font-mono truncate">{item.userEmail}</span>
              )}
            </div>
          </div>
        );
      }
    },
    { 
      key: 'action', 
      header: 'الإجراء',
      cell: (item: AuditLog) => {
        const info = actionLabels[item.action] || { label: item.action, bg: 'bg-gray-100', text: 'text-gray-800' };
        return (
          <span className={`px-2.5 py-1 rounded-full text-xs font-semibold ${info.bg} ${info.text}`}>
            {info.label}
          </span>
        );
      }
    },
    { 
      key: 'details', 
      header: 'التفاصيل',
      cell: (item: AuditLog) => (
        <span className="text-sm text-gray-700 block max-w-md break-words">
          {item.details}
        </span>
      )
    },
    { 
      key: 'timestamp', 
      header: 'التاريخ والوقت', 
      cell: (item: AuditLog) => new Date(item.timestamp).toLocaleString('ar-EG') 
    }
  ];

  return (
    <div className="p-6 rtl bg-gray-50 min-h-screen" dir="rtl">
      <div className="flex justify-between items-center mb-6">
        <h2 className="text-2xl font-bold text-gray-900">سجل التدقيق والأمان</h2>
        <span className="text-sm text-gray-500 font-medium">إجمالي السجلات: {totalCount}</span>
      </div>
      
      <div className="mb-6 max-w-md">
        <SearchBox 
          value={search} 
          onChange={(e) => setSearch(e.target.value)} 
          loading={isSearching}
          placeholder="ابحث في الإجراء، التفاصيل أو المستخدم..."
        />
      </div>

      <div className={`bg-white rounded-lg shadow-sm border border-gray-100 ${isSearching ? 'opacity-70 transition-opacity' : 'transition-opacity'}`}>
        <DataTable 
          columns={columns} 
          data={logs} 
          currentPage={page}
          totalPages={totalPages}
          onPageChange={setPage}
        />
      </div>
    </div>
  );
};
