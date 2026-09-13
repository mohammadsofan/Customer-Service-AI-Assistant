import { useEffect, useState } from 'react';
import analyticsService, { AnalyticsOverview, KnowledgeAnalytics, UnansweredQuestion } from '../services/analyticsService';
import { LoadingState } from '../components/LoadingState';
import { ErrorState } from '../components/ErrorState';
import { DataTable } from '../components/DataTable';

export const AnalyticsPage = () => {
  const [overview, setOverview] = useState<AnalyticsOverview | null>(null);
  const [knowledge, setKnowledge] = useState<KnowledgeAnalytics[]>([]);
  const [unanswered, setUnanswered] = useState<UnansweredQuestion[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchData = async () => {
      try {
        const [overviewData, knowledgeData, unansweredData] = await Promise.all([
          analyticsService.getOverview(),
          analyticsService.getKnowledgeAnalytics(),
          analyticsService.getUnanswered()
        ]);
        setOverview(overviewData);
        setKnowledge(knowledgeData);
        setUnanswered(unansweredData);
        setError(null);
      } catch (err) {
        setError('حدث خطأ أثناء تحميل البيانات');
      } finally {
        setLoading(false);
      }
    };
    fetchData();
  }, []);

  if (loading) return <LoadingState />;
  if (error) return <ErrorState message={error} onRetry={() => window.location.reload()} />;
  if (!overview) return null;

  const knowledgeColumns = [
    { key: 'categoryName', header: 'اسم التصنيف' },
    { key: 'usageCount', header: 'مرات الاستخدام' }
  ];

  const unansweredColumns = [
    { key: 'question', header: 'نص السؤال' },
    { key: 'timestamp', header: 'الوقت', cell: (item: UnansweredQuestion) => new Date(item.timestamp || Date.now()).toLocaleString('ar-EG') },
    { key: 'employeeId', header: 'الموظف', cell: (item: UnansweredQuestion) => item.employeeId || 'غير متوفر' }
  ];

  return (
    <div className="p-6 rtl bg-gray-50 min-h-screen" dir="rtl">
      <h2 className="text-2xl font-bold mb-6">الإحصائيات</h2>
      
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-8">
        <MetricCard title="إجمالي الأسئلة" value={overview.totalQuestions || 0} />
        <MetricCard title="تمت الإجابة" value={overview.answeredQuestions || 0} />
        <MetricCard title="لا توجد إجابة" value={overview.unansweredQuestions || 0} />
        <MetricCard title="نسبة النجاح" value={`${((overview.successRate ?? 0) * 100).toFixed(1)}%`} />
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
        <div className="bg-white p-6 rounded-lg shadow-sm border border-gray-100">
          <h3 className="text-xl font-bold mb-4">إحصائيات المعرفة</h3>
          <DataTable columns={knowledgeColumns} data={knowledge.map(k => ({ ...k, id: k.categoryId }))} />
        </div>
        
        <div className="bg-white p-6 rounded-lg shadow-sm border border-gray-100">
          <h3 className="text-xl font-bold mb-4">أسئلة غير مجابة</h3>
          <DataTable columns={unansweredColumns} data={unanswered} />
        </div>
      </div>
    </div>
  );
};

const MetricCard = ({ title, value }: { title: string; value: number | string }) => (
  <div className="bg-white p-6 rounded-lg shadow-sm border border-gray-100 text-center">
    <h3 className="text-gray-500 text-sm font-medium mb-2">{title}</h3>
    <p className="text-3xl font-bold text-blue-600">{value}</p>
  </div>
);
