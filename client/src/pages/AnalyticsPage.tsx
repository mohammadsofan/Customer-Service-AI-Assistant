import { useEffect, useState } from 'react';
import analyticsService, { 
  AnalyticsOverview, 
  KnowledgeAnalytics, 
  UnansweredQuestion, 
  CategoryAnalytics 
} from '../services/analyticsService';
import { LoadingState } from '../components/LoadingState';
import { ErrorState } from '../components/ErrorState';
import { DataTable } from '../components/DataTable';
import { Layers } from 'lucide-react';

export const AnalyticsPage = () => {
  const [overview, setOverview] = useState<AnalyticsOverview | null>(null);
  const [knowledge, setKnowledge] = useState<KnowledgeAnalytics[]>([]);
  const [categories, setCategories] = useState<CategoryAnalytics[]>([]);
  const [unanswered, setUnanswered] = useState<UnansweredQuestion[]>([]);
  const [sortOrder, setSortOrder] = useState<'desc' | 'asc'>('desc');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchData = async () => {
      try {
        const [overviewData, knowledgeData, categoriesData, unansweredData] = await Promise.all([
          analyticsService.getOverview(),
          analyticsService.getKnowledgeAnalytics(),
          analyticsService.getCategoryAnalytics(),
          analyticsService.getUnanswered()
        ]);
        setOverview(overviewData);
        setKnowledge(knowledgeData);
        setCategories(categoriesData);
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

  const categoryColumns = [
    { 
      key: 'categoryName', 
      header: 'اسم التصنيف',
      cell: (item: CategoryAnalytics) => (
        <div className="flex items-center gap-2">
          <div className="w-8 h-8 rounded-xl bg-blue-50 text-blue-600 flex items-center justify-center shrink-0">
            <Layers className="w-4 h-4" />
          </div>
          <span className="font-semibold text-gray-900">{item.categoryName}</span>
        </div>
      )
    },
    { 
      key: 'scenarioCount', 
      header: 'عدد السيناريوهات',
      cell: (item: CategoryAnalytics) => (
        <span className="inline-block px-2.5 py-1 rounded-lg text-xs font-semibold bg-gray-100 text-gray-700">
          {item.scenarioCount} سيناريو
        </span>
      )
    },
    { 
      key: 'questionCount', 
      header: 'الاستفسارات المرتبطة',
      cell: (item: CategoryAnalytics) => (
        <span className="font-bold text-gray-900">{item.questionCount}</span>
      )
    },
    { 
      key: 'percentage', 
      header: 'نسبة الاستخدام',
      cell: (item: CategoryAnalytics) => (
        <div className="flex items-center gap-3 min-w-[140px]">
          <div className="w-full bg-gray-100 rounded-full h-2.5 overflow-hidden">
            <div 
              className="bg-[#0055b8] h-2.5 rounded-full transition-all duration-500" 
              style={{ width: `${Math.min(item.percentage, 100)}%` }} 
            />
          </div>
          <span className="text-xs font-bold text-gray-700 w-10 text-left shrink-0">
            {item.percentage}%
          </span>
        </div>
      )
    }
  ];

  const knowledgeColumns = [
    { 
      key: 'scenarioName', 
      header: 'اسم السيناريو',
      cell: (item: KnowledgeAnalytics) => (
        <span className="font-semibold text-gray-900 block max-w-xs truncate" title={item.scenarioName}>
          {item.scenarioName}
        </span>
      )
    },
    { 
      key: 'categoryName', 
      header: 'التصنيف',
      cell: (item: KnowledgeAnalytics) => (
        <span className="inline-block px-2 py-0.5 rounded-md text-xs font-medium bg-blue-50 text-blue-700 border border-blue-100">
          {item.categoryName || 'غير مصنف'}
        </span>
      )
    },
    { 
      key: 'usageCount', 
      header: 'مرات الاستخدام',
      cell: (item: KnowledgeAnalytics) => (
        <span className="font-bold text-gray-800">{item.usageCount ?? 0}</span>
      )
    },
    { 
      key: 'avgSimilarityScore', 
      header: 'متوسط التطابق',
      cell: (item: KnowledgeAnalytics) => {
        const score = Math.round((item.avgSimilarityScore ?? 0) * 100);
        return (
          <span className={`text-xs font-bold px-2 py-0.5 rounded-md ${
            score >= 75 ? 'bg-emerald-50 text-emerald-700 border border-emerald-200' :
            score >= 40 ? 'bg-amber-50 text-amber-700 border border-amber-200' :
            'bg-gray-50 text-gray-600 border border-gray-200'
          }`}>
            {score}%
          </span>
        );
      }
    }
  ];

  const unansweredColumns = [
    { 
      key: 'question', 
      header: 'نص السؤال',
      cell: (item: UnansweredQuestion) => (
        <span className="font-medium text-gray-900 block max-w-xs truncate" title={item.question || item.questionText}>
          {item.question || item.questionText}
        </span>
      )
    },
    { 
      key: 'employee', 
      header: 'الموظف', 
      cell: (item: UnansweredQuestion) => {
        const name = item.employeeName || (item.employeeId ? 'موظف' : 'غير متوفر');
        return (
          <div className="flex items-center gap-2">
            <div className="w-7 h-7 rounded-full bg-blue-50 text-blue-600 font-bold text-xs flex items-center justify-center border border-blue-200 shrink-0">
              {item.employeeName ? item.employeeName.charAt(0).toUpperCase() : '؟'}
            </div>
            <div className="flex flex-col min-w-0">
              <span className="font-medium text-gray-900 text-sm truncate">{name}</span>
              {item.employeeEmail && (
                <span className="text-xs text-gray-400 truncate">{item.employeeEmail}</span>
              )}
            </div>
          </div>
        );
      }
    },
    { 
      key: 'frequency', 
      header: 'التكرار',
      cell: (item: UnansweredQuestion) => (
        <span className="inline-block px-2 py-0.5 rounded-full text-xs font-semibold bg-amber-50 text-amber-700 border border-amber-200">
          {item.frequency || 1}
        </span>
      )
    },
    { 
      key: 'timestamp', 
      header: 'التاريخ', 
      cell: (item: UnansweredQuestion) => {
        if (!item.timestamp) return 'غير متوفر';
        const d = new Date(item.timestamp);
        return (
          <div className="flex flex-col text-xs text-gray-600">
            <span className="font-medium text-gray-800">{d.toLocaleDateString('ar-EG')}</span>
            <span className="text-gray-400">{d.toLocaleTimeString('ar-EG', { hour: '2-digit', minute: '2-digit' })}</span>
          </div>
        );
      }
    }
  ];

  const sortedUnanswered = [...unanswered].sort((a, b) => {
    const timeA = new Date(a.timestamp || 0).getTime();
    const timeB = new Date(b.timestamp || 0).getTime();
    return sortOrder === 'desc' ? timeB - timeA : timeA - timeB;
  });

  return (
    <div className="p-6 rtl bg-[#f5f5f7] min-h-screen" dir="rtl">
      <h2 className="text-2xl font-bold mb-6 text-gray-900">التحليلات والتقارير</h2>
      
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-8">
        <MetricCard title="إجمالي الأسئلة" value={overview.totalQuestions || 0} color="text-slate-900" />
        <MetricCard title="تمت الإجابة" value={overview.answeredQuestions || 0} color="text-[#3b680c]" />
        <MetricCard title="لا توجد إجابة" value={overview.unansweredQuestions || 0} color="text-[#b34f07]" />
        <MetricCard title="نسبة النجاح" value={`${((overview.successRate ?? 0) * 100).toFixed(1)}%`} color="text-[#0055b8]" />
      </div>

      {/* Category Breakdown Section */}
      <div className="bg-white p-6 rounded-2xl shadow-xs border border-slate-200/80 mb-8">
        <div className="flex items-center justify-between mb-4">
          <div className="flex items-center gap-2">
            <Layers className="w-5 h-5 text-[#0055b8]" />
            <h3 className="text-xl font-bold text-slate-900">توزيع الاستفسارات حسب التصنيفات</h3>
          </div>
          <span className="text-xs text-gray-500 bg-gray-50 px-3 py-1 rounded-full border border-gray-200">
            {categories.length} تصنيفات نشطة
          </span>
        </div>
        <DataTable 
          columns={categoryColumns} 
          data={categories.map(c => ({ ...c, id: c.categoryId }))} 
        />
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
        <div className="bg-white p-6 rounded-2xl shadow-xs border border-slate-200/80">
          <h3 className="text-xl font-bold mb-4 text-slate-900">إحصائيات السيناريوهات (المعرفة)</h3>
          <DataTable 
            columns={knowledgeColumns} 
            data={knowledge.map(k => ({ ...k, id: k.scenarioId || k.id }))} 
          />
        </div>
        
        <div className="bg-white p-6 rounded-2xl shadow-xs border border-slate-200/80">
          <div className="flex items-center justify-between mb-4">
            <h3 className="text-xl font-bold text-slate-900">أسئلة غير مجابة</h3>
            <button
              type="button"
              onClick={() => setSortOrder(prev => prev === 'desc' ? 'asc' : 'desc')}
              className="text-xs text-[#0055b8] hover:text-[#004699] bg-[#0055b8]/5 hover:bg-[#0055b8]/10 px-3 py-1.5 rounded-xl font-medium transition-colors border border-[#0055b8]/20 flex items-center gap-1.5 cursor-pointer"
              title="تغيير اتجاه الترتيب"
            >
              <span>الترتيب بالتاريخ:</span>
              <span className="font-semibold">{sortOrder === 'desc' ? 'الأحدث أولاً ↓' : 'الأقدم أولاً ↑'}</span>
            </button>
          </div>
          <DataTable columns={unansweredColumns} data={sortedUnanswered} />
        </div>
      </div>
    </div>
  );
};

const MetricCard = ({ title, value, color = 'text-slate-900' }: { title: string; value: number | string; color?: string }) => (
  <div className="bg-white p-6 rounded-2xl shadow-xs border border-slate-200/80 text-center">
    <h3 className="text-slate-500 text-sm font-medium mb-2">{title}</h3>
    <p className={`text-3xl font-extrabold ${color}`}>{value}</p>
  </div>
);
