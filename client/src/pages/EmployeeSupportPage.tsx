import React, { useState } from 'react';
import supportService from '../services/supportService';
import type { QuestionResponse } from '../services/supportService';
import { Button } from '../components/Button';
import { Textarea } from '../components/Textarea';
import { Alert } from '../components/Alert';
import { LoadingState } from '../components/LoadingState';
import { CheckCircle2, XCircle, FileText, RefreshCw } from 'lucide-react';

type FlowState = 'NEW' | 'PROCESSING' | 'ANSWERED' | 'NO_ANSWER' | 'FAILED';

export const EmployeeSupportPage: React.FC = () => {
  const [flowState, setFlowState] = useState<FlowState>('NEW');
  const [problem, setProblem] = useState('');
  const [response, setResponse] = useState<QuestionResponse | null>(null);
  const [error, setError] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!problem.trim()) return;

    setFlowState('PROCESSING');
    setError('');

    try {
      const result = await supportService.submitQuestion({ problem });
      setResponse(result);

      if (result.status === 'NoAnswer' || !result.answered) {
        setFlowState('NO_ANSWER');
      } else if (result.status === 'Failed') {
        setFlowState('FAILED');
      } else {
        setFlowState('ANSWERED');
      }
    } catch (err: any) {
      setFlowState('FAILED');
      setError('حدث خطأ أثناء محاولة إرسال المشكلة.');
    }
  };

  const handleReset = () => {
    setFlowState('NEW');
    setProblem('');
    setResponse(null);
    setError('');
  };

  return (
    <div className="max-w-4xl mx-auto space-y-6" dir="rtl">
      {/* Input Section */}
      <div className="bg-white p-6 rounded-xl shadow-sm border border-gray-200">
        <h2 className="text-xl font-bold text-gray-900 mb-4">ما المشكلة التي يواجهها العميل؟</h2>
        <form onSubmit={handleSubmit} className="space-y-4">
          <Textarea
            value={problem}
            onChange={(e) => setProblem(e.target.value)}
            placeholder="اكتب تفاصيل المشكلة هنا..."
            className="min-h-[120px]"
            disabled={flowState !== 'NEW'}
          />
          
          {flowState === 'NEW' && (
            <div className="flex justify-end">
              <Button type="submit" disabled={!problem.trim()}>
                إرسال
              </Button>
            </div>
          )}
        </form>
      </div>

      {/* Processing State */}
      {flowState === 'PROCESSING' && (
        <div className="bg-white p-8 rounded-xl shadow-sm border border-gray-200 text-center space-y-4">
          <LoadingState text="جاري تحليل المشكلة والبحث عن الحل المناسب..." />
        </div>
      )}

      {/* Answered State */}
      {flowState === 'ANSWERED' && response && (
        <div className="bg-white p-6 rounded-xl shadow-sm border border-green-200 bg-green-50/30">
          <div className="flex items-center gap-2 text-green-700 mb-4">
            <CheckCircle2 className="h-6 w-6" />
            <h3 className="text-xl font-bold">الحل المقترح</h3>
          </div>
          
          <div className="bg-white p-4 rounded-lg border border-green-100 mb-4 text-gray-800 whitespace-pre-wrap">
            {response.answer}
          </div>

          {response.sourceScenario && (
            <div className="flex items-center gap-2 text-sm text-gray-500 mt-4 bg-gray-50 p-2 rounded border border-gray-100">
              <FileText className="h-4 w-4" />
              <span>المصدر: {response.sourceScenario}</span>
              {response.confidenceScore !== undefined && (
                <span className="mr-auto px-2 py-1 bg-white rounded shadow-sm text-xs border border-gray-200">
                  الدقة: {(response.confidenceScore * 100).toFixed(0)}%
                </span>
              )}
            </div>
          )}

          <div className="mt-6 flex justify-start">
            <Button onClick={handleReset} variant="outline" className="flex items-center gap-2">
              <RefreshCw className="h-4 w-4" />
              سؤال جديد
            </Button>
          </div>
        </div>
      )}

      {/* No Answer State */}
      {flowState === 'NO_ANSWER' && (
        <div className="bg-white p-6 rounded-xl shadow-sm border border-yellow-200 bg-yellow-50/30">
          <div className="flex items-center gap-2 text-yellow-700 mb-4">
            <XCircle className="h-6 w-6" />
            <h3 className="text-xl font-bold">عذراً، لم يتم العثور على حل</h3>
          </div>
          
          <Alert 
            type="warning" 
            message="لا أملك إجابة بخصوص هذا الموضوع. يرجى التواصل مع Back Office للحصول على مزيد من المساعدة بخصوص هذه المشكلة." 
          />

          <div className="mt-6 flex justify-start">
            <Button onClick={handleReset} variant="outline" className="flex items-center gap-2">
              <RefreshCw className="h-4 w-4" />
              سؤال جديد
            </Button>
          </div>
        </div>
      )}

      {/* Failed State */}
      {flowState === 'FAILED' && (
        <div className="bg-white p-6 rounded-xl shadow-sm border border-red-200 bg-red-50/30">
          <div className="flex items-center gap-2 text-red-700 mb-4">
            <XCircle className="h-6 w-6" />
            <h3 className="text-xl font-bold">حدث خطأ</h3>
          </div>
          
          <Alert 
            type="error" 
            message={error || "لا أملك إجابة بخصوص هذا الموضوع. يرجى التواصل مع Back Office للحصول على مزيد من المساعدة بخصوص هذه المشكلة."} 
          />

          <div className="mt-6 flex justify-start">
            <Button onClick={handleReset} variant="outline" className="flex items-center gap-2">
              <RefreshCw className="h-4 w-4" />
              سؤال جديد
            </Button>
          </div>
        </div>
      )}
    </div>
  );
};
