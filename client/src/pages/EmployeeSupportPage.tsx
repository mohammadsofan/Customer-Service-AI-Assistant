import React, { useState, useEffect } from 'react';
import supportService, { type QuestionResponse, type QuestionHistoryDto } from '../services/supportService';
import { 
  Bot, 
  Send, 
  RotateCcw, 
  CheckCircle2, 
  AlertTriangle, 
  FileText, 
  History, 
  Sparkles, 
  HelpCircle, 
  ArrowLeft, 
  Clock, 
  Check, 
  X,
  ExternalLink,
  ChevronRight
} from 'lucide-react';

type FlowState = 'NEW' | 'PROCESSING' | 'ANSWERED' | 'NO_ANSWER' | 'FAILED';

const quickSuggestions = [
  'ضعف إشارة الشبكة أو انقطاع الخدمة في منطقة العميل',
  'العميل يريد إعادة تعيين كلمة المرور الخاصة بحسابه',
  'مشكلة في تفعيل شريحة SIM الجديدة بعد الشراء',
  'استفسار عن رصيد الفاتورة ومشاكل الدفع والتحصيل'
];

export const EmployeeSupportPage: React.FC = () => {
  const [flowState, setFlowState] = useState<FlowState>('NEW');
  const [problem, setProblem] = useState('');
  const [response, setResponse] = useState<QuestionResponse | null>(null);
  const [error, setError] = useState('');
  const [history, setHistory] = useState<QuestionHistoryDto[]>([]);
  const [isHistoryLoading, setIsHistoryLoading] = useState(false);

  const fetchHistory = async () => {
    try {
      setIsHistoryLoading(true);
      const res = await supportService.getQuestionHistory({ page: 1, pageSize: 5 });
      setHistory(res?.items || []);
    } catch {
      // History is non-blocking
    } finally {
      setIsHistoryLoading(false);
    }
  };

  useEffect(() => {
    fetchHistory();
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!problem.trim()) return;

    setFlowState('PROCESSING');
    setError('');

    try {
      const result = await supportService.submitQuestion({ problem });
      setResponse(result);

      if (result.status === 'NoAnswer' || !result.answered || result.escalated) {
        setFlowState('NO_ANSWER');
      } else if (result.status === 'Failed') {
        setFlowState('FAILED');
      } else {
        setFlowState('ANSWERED');
      }
      fetchHistory();
    } catch (err: any) {
      setFlowState('FAILED');
      setError('حدث خطأ أثناء معالجة السؤال بواسطة النظام. يرجى المحاولة لاحقاً.');
    }
  };

  const handleReset = () => {
    setFlowState('NEW');
    setProblem('');
    setResponse(null);
    setError('');
  };

  const selectSuggestion = (text: string) => {
    if (flowState === 'NEW') {
      setProblem(text);
    }
  };

  return (
    <div className="space-y-8" dir="rtl">
      {/* Hero Welcome */}
      <div className="text-center max-w-2xl mx-auto space-y-2 mb-2">
        <div className="inline-flex items-center gap-2 px-3.5 py-1 rounded-full bg-[#76bc21]/15 border border-[#76bc21]/30 text-xs font-bold text-[#3b6b0c]">
          <Sparkles className="w-3.5 h-3.5 text-[#76bc21]" />
          <span>نظام الإجابات المعرفية الفورية (RAG)</span>
        </div>
        <h2 className="text-2xl sm:text-3xl font-extrabold text-slate-900 tracking-tight">
          ما المشكلة التي يواجهها العميل؟
        </h2>
        <p className="text-sm text-slate-500">
          اكتب استفسار العميل باللغة العربية وسيقوم المساعد بمطابقته فورياً مع قاعدة المعرفة المعتمدة
        </p>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-12 gap-8 items-start">
        {/* Main Interaction Area (8 cols) */}
        <div className="lg:col-span-8 space-y-6">
          {/* Form Card */}
          <div className="bg-white rounded-2xl border border-slate-200/80 shadow-xs p-6 sm:p-7 relative">
            <form onSubmit={handleSubmit} className="space-y-4">
              <div className="flex items-center justify-between mb-1">
                <label className="text-sm font-bold text-slate-800 flex items-center gap-2">
                  <HelpCircle className="w-4 h-4 text-[#0055b8]" />
                  <span>نص المشكلة أو الاستفسار</span>
                </label>
                {flowState === 'NEW' && problem && (
                  <button
                    type="button"
                    onClick={() => setProblem('')}
                    className="text-xs text-slate-400 hover:text-slate-600 cursor-pointer"
                  >
                    مسح النص
                  </button>
                )}
              </div>

              <div className="relative">
                <textarea
                  value={problem}
                  onChange={(e) => setProblem(e.target.value)}
                  placeholder="اكتب تفاصيل المشكلة هنا..."
                  rows={4}
                  disabled={flowState !== 'NEW'}
                  className={`w-full p-4 rounded-xl text-slate-900 placeholder:text-slate-400 border transition-all text-sm leading-relaxed focus:outline-none ${
                    flowState !== 'NEW'
                      ? 'bg-slate-50 border-slate-200 text-slate-500 cursor-not-allowed'
                      : 'bg-white border-slate-200 focus:border-[#76bc21] focus:ring-4 focus:ring-[#76bc21]/15'
                  }`}
                />
              </div>

              {/* Quick suggestions when input is empty */}
              {flowState === 'NEW' && !problem && (
                <div className="space-y-2 pt-1">
                  <div className="text-xs font-semibold text-slate-400">
                    أمثلة شائعة للاختيار السريع:
                  </div>
                  <div className="flex flex-wrap gap-2">
                    {quickSuggestions.map((s, idx) => (
                      <button
                        key={idx}
                        type="button"
                        onClick={() => selectSuggestion(s)}
                        className="text-xs px-3.5 py-2 rounded-xl bg-slate-50 hover:bg-[#76bc21]/10 border border-slate-200/80 hover:border-[#76bc21]/40 text-slate-700 hover:text-[#3d6c0f] transition-all cursor-pointer text-right flex items-center gap-1.5"
                      >
                        <ChevronRight className="w-3 h-3 text-slate-400" />
                        <span>{s}</span>
                      </button>
                    ))}
                  </div>
                </div>
              )}

              {/* Submit Button (Only in NEW state) */}
              {flowState === 'NEW' && (
                <div className="flex justify-end pt-2">
                  <button
                    type="submit"
                    disabled={!problem.trim()}
                    className="px-6 py-3 rounded-xl bg-[#76bc21] hover:bg-[#67a61d] active:bg-[#5b9419] text-white font-bold text-sm shadow-md shadow-[#76bc21]/25 hover:shadow-[#76bc21]/40 disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2 cursor-pointer transition-all active:scale-[0.98]"
                  >
                    <span>إرسال</span>
                    <Send className="w-4 h-4 rotate-180" />
                  </button>
                </div>
              )}
            </form>
          </div>

          {/* Processing / Thinking State */}
          {flowState === 'PROCESSING' && (
            <div className="bg-white rounded-2xl border border-[#76bc21]/30 shadow-lg shadow-[#76bc21]/5 p-8 text-center space-y-4 animate-pulse">
              <div className="w-14 h-14 rounded-2xl bg-[#76bc21]/10 text-[#76bc21] flex items-center justify-center mx-auto shadow-inner">
                <Bot className="w-8 h-8 animate-bounce text-[#76bc21]" />
              </div>
              <div className="space-y-1">
                <h3 className="text-base font-bold text-slate-900">
                  جاري تحليل المشكلة والبحث عن الحل المناسب...
                </h3>
                <p className="text-xs text-slate-500">
                  نقوم بتحليل الكلمات المفتاحية ومطابقة النواقل الدلالية في قاعدة المعرفة المعتمدة
                </p>
              </div>
              <div className="w-48 h-1.5 bg-slate-100 rounded-full mx-auto overflow-hidden">
                <div className="w-full h-full bg-[#76bc21] rounded-full animate-[shimmer_1.5s_infinite]" />
              </div>
            </div>
          )}

          {/* Answered State (Success) */}
          {flowState === 'ANSWERED' && response && (
            <div className="bg-white rounded-2xl border border-[#76bc21]/40 shadow-md shadow-[#76bc21]/5 p-6 sm:p-8 space-y-6">
              {/* Header */}
              <div className="flex items-center justify-between pb-4 border-b border-slate-100">
                <div className="flex items-center gap-3">
                  <div className="w-10 h-10 rounded-xl bg-[#76bc21]/15 text-[#3b680c] flex items-center justify-center">
                    <CheckCircle2 className="w-6 h-6" />
                  </div>
                  <div>
                    <h3 className="text-lg font-bold text-slate-900">
                      الحل المعتمد من قاعدة المعرفة
                    </h3>
                    <p className="text-xs text-[#3b680c] font-medium">
                      تم التحقق والمطابقة بنجاح
                    </p>
                  </div>
                </div>

                {response.confidenceScore !== undefined && (
                  <div className="text-left">
                    <div className="text-xs text-slate-400">نسبة التطابق</div>
                    <div className="text-sm font-bold text-[#3b680c]">
                      {(response.confidenceScore * 100).toFixed(0)}%
                    </div>
                  </div>
                )}
              </div>

              {/* Answer Content */}
              <div className="p-5 rounded-xl bg-slate-50/80 border border-slate-200/60 text-slate-800 text-sm leading-relaxed whitespace-pre-wrap font-normal">
                {response.answer}
              </div>

              {/* Steps (if available) */}
              {response.steps && response.steps.length > 0 && (
                <div className="space-y-3">
                  <div className="text-xs font-bold text-slate-700 uppercase tracking-wide">
                    خطوات الحل الإجرائية:
                  </div>
                  <div className="space-y-2">
                    {response.steps.map((step, idx) => (
                      <div
                        key={idx}
                        className="flex items-start gap-3 p-3.5 rounded-xl bg-white border border-slate-200/80 text-sm"
                      >
                        <span className="w-6 h-6 rounded-full bg-[#76bc21]/15 text-[#3b680c] text-xs font-bold flex items-center justify-center shrink-0 mt-0.5">
                          {idx + 1}
                        </span>
                        <span className="text-slate-700 leading-relaxed">{step}</span>
                      </div>
                    ))}
                  </div>
                </div>
              )}

              {/* Source Scenario Metadata */}
              {response.sourceScenario && (
                <div className="flex items-center gap-2 p-3 rounded-xl bg-slate-50 text-xs text-slate-600 border border-slate-200/60">
                  <FileText className="w-4 h-4 text-blue-600" />
                  <span className="font-semibold">السيناريو المرجعي:</span>
                  <span>{response.sourceScenario}</span>
                </div>
              )}

              {/* New Question Action Button */}
              <div className="pt-2">
                <button
                  onClick={handleReset}
                  className="px-6 py-3 rounded-xl bg-slate-900 hover:bg-[#76bc21] hover:text-white text-white font-bold text-sm transition-all flex items-center gap-2 cursor-pointer shadow-sm hover:shadow"
                >
                  <RotateCcw className="w-4 h-4" />
                  <span>بدء سؤال جديد</span>
                </button>
              </div>
            </div>
          )}

          {/* No Answer / Escalated State */}
          {flowState === 'NO_ANSWER' && (
            <div className="bg-white rounded-2xl border border-amber-200 shadow-md shadow-amber-500/5 p-6 sm:p-8 space-y-6">
              <div className="flex items-center gap-3 pb-4 border-b border-slate-100">
                <div className="w-10 h-10 rounded-xl bg-amber-50 text-amber-600 flex items-center justify-center">
                  <AlertTriangle className="w-6 h-6" />
                </div>
                <div>
                  <h3 className="text-lg font-bold text-slate-900">
                    عذراً، لم يتم العثور على حل
                  </h3>
                  <p className="text-xs text-amber-600 font-medium">
                    تنبيه: يتطلب التصعيد إلى Back Office
                  </p>
                </div>
              </div>

              {/* Standardized Arabic Escalation Alert */}
              <div className="p-5 rounded-xl bg-amber-50/70 border border-amber-200 text-amber-950 font-medium text-sm leading-relaxed">
                لا أملك إجابة بخصوص هذا الموضوع. يرجى التواصل مع Back Office للحصول على مزيد من المساعدة بخصوص هذه المشكلة.
              </div>

              <div className="p-4 rounded-xl bg-slate-50 border border-slate-200/80 text-xs text-slate-600 space-y-1.5">
                <div className="font-bold text-slate-800">إجراءات الموظف الموصى بها:</div>
                <ul className="list-disc list-inside space-y-1">
                  <li>تواصل مع ال Back Office.</li>
                  <li>تزويدهم بتفاصيل المشكلة ورقم اشتراك العميل بدقة.</li>
                  <li>تم تسجيل هذا السؤال آلياً لمراجعته من قبل إدارة المعرفة لاحقاً.</li>
                </ul>
              </div>

              <div className="pt-2">
                <button
                  onClick={handleReset}
                  className="px-6 py-3 rounded-xl bg-slate-900 hover:bg-[#76bc21] hover:text-white text-white font-bold text-sm transition-all flex items-center gap-2 cursor-pointer shadow-sm"
                >
                  <RotateCcw className="w-4 h-4" />
                  <span>بدء سؤال جديد</span>
                </button>
              </div>
            </div>
          )}

          {/* Failed State */}
          {flowState === 'FAILED' && (
            <div className="bg-white rounded-2xl border border-rose-200 p-6 sm:p-8 space-y-6">
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 rounded-xl bg-rose-50 text-rose-600 flex items-center justify-center">
                  <AlertTriangle className="w-6 h-6" />
                </div>
                <div>
                  <h3 className="text-lg font-bold text-slate-900">تعذر إكمال الطلب</h3>
                  <p className="text-xs text-rose-600">{error}</p>
                </div>
              </div>

              <div className="pt-2">
                <button
                  onClick={handleReset}
                  className="px-6 py-3 rounded-xl bg-slate-900 hover:bg-[#76bc21] hover:text-white text-white font-bold text-sm transition-all flex items-center gap-2 cursor-pointer"
                >
                  <RotateCcw className="w-4 h-4" />
                  <span>المحاولة مرة أخرى</span>
                </button>
              </div>
            </div>
          )}
        </div>

        {/* Sidebar: Recent History & Guidance (4 cols) */}
        <div className="lg:col-span-4 space-y-6">
          {/* History Card */}
          <div className="bg-white rounded-2xl border border-slate-200/80 shadow-xs p-6 space-y-4">
            <div className="flex items-center justify-between">
              <h3 className="text-sm font-bold text-slate-900 flex items-center gap-2">
                <History className="w-4 h-4 text-[#0055b8]" />
                <span>سجل استفساراتك الأخيرة</span>
              </h3>
              <button
                onClick={fetchHistory}
                disabled={isHistoryLoading}
                className="text-xs text-[#0055b8] hover:text-[#004699] font-medium cursor-pointer"
              >
                تحديث
              </button>
            </div>

            {history.length === 0 ? (
              <div className="py-8 text-center text-xs text-slate-400">
                لا توجد استفسارات سابقة مسجلة اليوم.
              </div>
            ) : (
              <div className="space-y-2.5">
                {history.map((item) => (
                  <div
                    key={item.id}
                    className="p-3 rounded-xl bg-slate-50/70 border border-slate-200/60 text-xs space-y-1.5 hover:border-[#76bc21]/50 transition-colors"
                  >
                    <div className="font-semibold text-slate-800 line-clamp-2">
                      {item.questionText}
                    </div>
                    <div className="flex items-center justify-between text-[11px] text-slate-400">
                      <span className="flex items-center gap-1">
                        <Clock className="w-3 h-3" />
                        <span>{new Date(item.createdAt).toLocaleTimeString('ar-EG', { hour: '2-digit', minute: '2-digit' })}</span>
                      </span>
                      <span
                        className={`px-2 py-0.5 rounded-full font-bold ${
                          item.status === 'Answered'
                            ? 'bg-[#76bc21]/15 text-[#3b680c]'
                            : 'bg-[#f4771d]/15 text-[#b34f07]'
                        }`}
                      >
                        {item.status === 'Answered' ? 'تمت الإجابة' : 'تم التصعيد'}
                      </span>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>

          {/* Quick Guide card */}
          <div className="p-5 rounded-2xl bg-white border border-slate-200/80 shadow-xs text-xs text-slate-700 space-y-2.5">
            <div className="font-bold text-slate-900 flex items-center gap-1.5 text-sm">
              <Bot className="w-4 h-4 text-[#76bc21]" />
              <span>تعليمات استخدام المساعد</span>
            </div>
            <p className="text-slate-600 leading-relaxed">
              يقوم المساعد بمقارنة سؤالك بدقة مع سيناريوهات الدعم المعتمدة لتقديم إجابة موحدة وموثوقة لكافة موظفي خدمة العملاء.
            </p>
            <div className="pt-1 text-[11px] text-[#0055b8] font-semibold">
              إذا لم تكن المعرفة متوفرة، فسيتم توجيهك فوراً للتصعيد دون أي تكهنات أو معلومات خاطئة.
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
