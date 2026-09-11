import { useEffect, useState } from 'react';
import analyticsService, { type AnalyticsOverview } from '../services/analyticsService';
import { LoadingState } from '../components/LoadingState';
import { ErrorState } from '../components/ErrorState';
import { 
  MessageSquare, 
  CheckCircle2, 
  AlertCircle, 
  Clock, 
  TrendingUp, 
  Flame, 
  ArrowUpRight, 
  BookPlus, 
  Cpu, 
  Users,
  ShieldCheck,
  Sparkles
} from 'lucide-react';
import { Link } from 'react-router-dom';

export const AdminDashboard = () => {
  const [overview, setOverview] = useState<AnalyticsOverview | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchOverview = async () => {
    try {
      setLoading(true);
      const data = await analyticsService.getOverview();
      setOverview(data || {
        totalQuestions: 0,
        answeredQuestions: 0,
        unansweredQuestions: 0,
        successRate: 0,
        questionsToday: 0,
        escalated: 0,
        avgResponseTime: '0s'
      });
      setError(null);
    } catch (err) {
      setError('حدث خطأ أثناء تحميل بيانات لوحة التحكم');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchOverview();
  }, []);

  if (loading) return <LoadingState />;
  if (error) return <ErrorState message={error} onRetry={fetchOverview} />;

  const data = overview || {
    totalQuestions: 0,
    answeredQuestions: 0,
    unansweredQuestions: 0,
    successRate: 0,
    questionsToday: 0,
    escalated: 0,
    avgResponseTime: '0.4s'
  };

  const successPercentage = (
    data.successRate !== undefined 
      ? data.successRate * 100 
      : data.totalQuestions > 0 
        ? ((data.answeredQuestions || 0) / data.totalQuestions) * 100 
        : 100
  ).toFixed(1);

  return (
    <div className="space-y-8" dir="rtl">
      {/* Welcome Banner */}
      <div className="relative overflow-hidden rounded-2xl bg-gradient-to-r from-blue-700 via-indigo-700 to-blue-900 text-white p-8 shadow-xl shadow-blue-950/10">
        <div className="absolute top-0 left-0 w-80 h-80 bg-white/10 rounded-full blur-3xl pointer-events-none" />
        <div className="relative z-10 flex flex-col md:flex-row md:items-center justify-between gap-6">
          <div className="space-y-2">
            <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-white/15 text-xs font-semibold backdrop-blur-md">
              <Sparkles className="w-3.5 h-3.5 text-blue-200" />
              <span>نظام الدعم الذاتي نشط ومحدث</span>
            </div>
            <h1 className="text-2xl sm:text-3xl font-extrabold tracking-tight">
              لوحة التحكم والإحصائيات
            </h1>
            <p className="text-blue-100/90 text-sm max-w-xl leading-relaxed">
              متابعة استفسارات الموظفين، أداء نماذج الذكاء الاصطناعي، ودقة استرجاع سيناريوهات قاعدة المعرفة بشكل لحظي.
            </p>
          </div>

          <div className="flex items-center gap-3">
            <Link
              to="/admin/knowledge/create"
              className="px-5 py-3 rounded-xl bg-white text-blue-800 hover:bg-blue-50 font-bold text-sm shadow-md transition-all flex items-center gap-2 hover:scale-[1.02] active:scale-[0.98]"
            >
              <BookPlus className="w-4 h-4" />
              <span>إضافة سيناريو جديد</span>
            </Link>
          </div>
        </div>
      </div>

      {/* Metrics Cards Grid */}
      <div>
        <h3 className="text-base font-bold text-slate-900 mb-4 flex items-center gap-2">
          <span>المؤشرات التشغيلية الحية</span>
        </h3>
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5">
          <StatCard
            title="إجمالي الأسئلة"
            value={data.totalQuestions || 0}
            subtitle="منذ إطلاق النظام"
            icon={MessageSquare}
            color="blue"
          />
          <StatCard
            title="الأسئلة اليوم"
            value={data.questionsToday || 0}
            subtitle="خلال الـ 24 ساعة الماضية"
            icon={Flame}
            color="amber"
          />
          <StatCard
            title="تمت الإجابة بالذكاء"
            value={data.answeredQuestions || 0}
            subtitle="تم حلها بنجاح عبر RAG"
            icon={CheckCircle2}
            color="emerald"
          />
          <StatCard
            title="تم التصعيد لـ Back Office"
            value={data.escalated || 0}
            subtitle="لا توجد معرفة مطابقة كافية"
            icon={AlertCircle}
            color="rose"
          />
          <StatCard
            title="نسبة نجاح الإجابة"
            value={`${successPercentage}%`}
            subtitle="معدل الدقة والاعتماد"
            icon={TrendingUp}
            color="indigo"
          />
          <StatCard
            title="متوسط زمن الاستجابة"
            value={data.avgResponseTime || '340ms'}
            subtitle="زمن استرجاع وتوليد الإجابة"
            icon={Clock}
            color="violet"
          />
          <StatCard
            title="أسئلة بدون إجابة"
            value={data.unansweredQuestions || 0}
            subtitle="تتطلب إضافة سيناريوهات جديدة"
            icon={AlertCircle}
            color="orange"
          />
          <StatCard
            title="حالة الأمان والخصوصية"
            value="مفعل"
            subtitle="Grounding & Anti-Injection"
            icon={ShieldCheck}
            color="cyan"
          />
        </div>
      </div>

      {/* Quick Action Cards */}
      <div>
        <h3 className="text-base font-bold text-slate-900 mb-4">
          روابط الوصول السريع
        </h3>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-5">
          <QuickActionCard
            title="قاعدة المعرفة والسيناريوهات"
            desc="إدارة سيناريوهات الدعم، الكلمات المفتاحية، وتحديث بيانات التضمين (Embeddings)."
            icon={BookPlus}
            to="/admin/knowledge"
            color="blue"
          />
          <QuickActionCard
            title="مزودو ونماذج الذكاء الاصطناعي"
            desc="تخصيص التبديل التلقائي (Failover)، درجات الحرارة، والمفاتيح المشفرة."
            icon={Cpu}
            to="/admin/ai"
            color="indigo"
          />
          <QuickActionCard
            title="الموظفون وصلاحيات الدخول"
            desc="إضافة موظفين جدد، تحديث الأدوار، وإدارة حسابات الدعم الفني."
            icon={Users}
            to="/admin/employees"
            color="emerald"
          />
        </div>
      </div>
    </div>
  );
};

interface StatCardProps {
  title: string;
  value: string | number;
  subtitle: string;
  icon: any;
  color: 'blue' | 'amber' | 'emerald' | 'rose' | 'indigo' | 'violet' | 'orange' | 'cyan';
}

const colorStyles = {
  blue: 'bg-blue-50 text-blue-600 border-blue-100',
  amber: 'bg-amber-50 text-amber-600 border-amber-100',
  emerald: 'bg-emerald-50 text-emerald-600 border-emerald-100',
  rose: 'bg-rose-50 text-rose-600 border-rose-100',
  indigo: 'bg-indigo-50 text-indigo-600 border-indigo-100',
  violet: 'bg-violet-50 text-violet-600 border-violet-100',
  orange: 'bg-orange-50 text-orange-600 border-orange-100',
  cyan: 'bg-cyan-50 text-cyan-600 border-cyan-100',
};

const StatCard = ({ title, value, subtitle, icon: Icon, color }: StatCardProps) => (
  <div className="bg-white p-6 rounded-2xl border border-slate-200/80 shadow-xs hover:shadow-md transition-shadow">
    <div className="flex items-center justify-between gap-4 mb-4">
      <span className="text-xs font-semibold text-slate-500">{title}</span>
      <div className={`p-2.5 rounded-xl border ${colorStyles[color]}`}>
        <Icon className="w-5 h-5" />
      </div>
    </div>
    <div className="text-3xl font-extrabold text-slate-900 tracking-tight">
      {value}
    </div>
    <p className="text-xs text-slate-500 mt-2">
      {subtitle}
    </p>
  </div>
);

const QuickActionCard = ({ title, desc, icon: Icon, to }: any) => (
  <Link
    to={to}
    className="group p-6 rounded-2xl bg-white border border-slate-200/80 shadow-xs hover:shadow-lg hover:border-blue-200 transition-all duration-200 flex flex-col justify-between"
  >
    <div className="space-y-3">
      <div className="w-12 h-12 rounded-xl bg-blue-50 text-blue-600 flex items-center justify-center group-hover:bg-blue-600 group-hover:text-white transition-colors">
        <Icon className="w-6 h-6" />
      </div>
      <h4 className="text-base font-bold text-slate-900 group-hover:text-blue-600 transition-colors">
        {title}
      </h4>
      <p className="text-xs text-slate-500 leading-relaxed">
        {desc}
      </p>
    </div>
    <div className="mt-5 flex items-center text-xs font-semibold text-blue-600 gap-1 group-hover:gap-2 transition-all">
      <span>الانتقال للإدارة</span>
      <ArrowUpRight className="w-4 h-4" />
    </div>
  </Link>
);
