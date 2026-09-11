import { useEffect, useState, useMemo } from 'react';
import auditService, { AuditLog } from '../services/auditService';
import { DataTable } from '../components/DataTable';
import { SearchBox } from '../components/SearchBox';
import { LoadingState } from '../components/LoadingState';
import { ErrorState } from '../components/ErrorState';

export const AuditLogPage = () => {
  const [logs, setLogs] = useState<AuditLog[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');
  const pageSize = 15;

  useEffect(() => {
    const fetchLogs = async () => {
      try {
        const data = await auditService.getAuditLogs();
        setLogs(data);
        setError(null);
      } catch (err) {
        setError('حدث خطأ أثناء تحميل سجل التدقيق');
      } finally {
        setLoading(false);
      }
    };
    fetchLogs();
  }, []);

  const filteredLogs = useMemo(() => {
    if (!search) return logs;
    return logs.filter(log => 
      log.action.includes(search) || 
      (log.details || '').includes(search) || 
      log.userId.includes(search)
    );
  }, [logs, search]);

  const paginatedLogs = useMemo(() => {
    const start = (page - 1) * pageSize;
    return filteredLogs.slice(start, start + pageSize);
  }, [filteredLogs, page]);

  if (loading) return <LoadingState />;
  if (error) return <ErrorState message={error} onRetry={() => window.location.reload()} />;

  const columns = [
    { key: 'id', header: 'المعرف' },
    { key: 'action', header: 'الإجراء' },
    { key: 'details', header: 'التفاصيل' },
    { key: 'userId', header: 'معرف المستخدم' },
    { key: 'timestamp', header: 'الوقت', cell: (item: AuditLog) => new Date(item.timestamp).toLocaleString('ar-EG') }
  ];

  return (
    <div className="p-6 rtl bg-gray-50 min-h-screen" dir="rtl">
      <h2 className="text-2xl font-bold mb-6">سجل التدقيق</h2>
      
      <div className="mb-6 max-w-md">
        <SearchBox 
          value={search} 
          onChange={(e) => {
            setSearch(e.target.value);
            setPage(1);
          }} 
          placeholder="ابحث في الإجراء، التفاصيل أو المستخدم..."
        />
      </div>

      <div className="bg-white rounded-lg shadow-sm border border-gray-100">
        <DataTable 
          columns={columns} 
          data={paginatedLogs} 
          currentPage={page}
          totalPages={Math.ceil(filteredLogs.length / pageSize)}
          onPageChange={setPage}
        />
      </div>
    </div>
  );
};
