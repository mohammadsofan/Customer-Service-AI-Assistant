import { useEffect, useState } from 'react';
import analyticsService, { AnalyticsOverview } from '../services/analyticsService';
import { LoadingState } from '../components/LoadingState';
import { ErrorState } from '../components/ErrorState';

export const AdminDashboard = () => {
  const [overview, setOverview] = useState<AnalyticsOverview | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchOverview = async () => {
      try {
        const data = await analyticsService.getOverview();
        setOverview(data);
        setError(null);
      } catch (err) {
        setError('حدث خطأ أثناء تحميل البيانات');
      } finally {
        setLoading(false);
      }
    };
    fetchOverview();
  }, []);

  if (loading) return <LoadingState />;
  if (error) return <ErrorState message={error} onRetry={() => window.location.reload()} />;
  if (!overview) return null;

  return (
    <div className="p-6 rtl" dir="rtl">
      <h2 className="text-2xl font-bold mb-6">لوحة التحكم</h2>
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
        <MetricCard title="إجمالي الأسئلة" value={overview.totalQuestions} />
        <MetricCard title="الأسئلة اليوم" value={overview.questionsToday || 0} />
        <MetricCard title="تمت الإجابة" value={overview.answeredQuestions} />
        <MetricCard title="لا توجد إجابة" value={overview.unansweredQuestions} />
        <MetricCard title="تم التصعيد" value={overview.escalated || 0} />
        <MetricCard title="متوسط وقت الاستجابة" value={overview.avgResponseTime || '0s'} />
        <MetricCard title="نسبة الإجابة" value={`${(overview.successRate * 100).toFixed(1)}%`} />
      </div>
    </div>
  );
};

const MetricCard = ({ title, value }: { title: string; value: number | string }) => (
  <div className="bg-white p-6 rounded-lg shadow-sm border border-gray-100 flex flex-col justify-center items-center">
    <h3 className="text-gray-500 text-sm font-medium mb-2 text-center">{title}</h3>
    <p className="text-3xl font-bold text-gray-800 text-center">{value}</p>
  </div>
);
