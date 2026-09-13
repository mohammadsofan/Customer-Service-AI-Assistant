import { useEffect, useState } from 'react';
import supportService, { QuestionHistoryDto } from '../services/supportService';
import { DataTable } from '../components/DataTable';
import { Modal } from '../components/Modal';
import { Select } from '../components/Select';
import { Input } from '../components/Input';
import { Button } from '../components/Button';

export const QuestionsPage = () => {
  const [questions, setQuestions] = useState<QuestionHistoryDto[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [status, setStatus] = useState('');
  const [date, setDate] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [selectedQuestion, setSelectedQuestion] = useState<QuestionHistoryDto | null>(null);

  const fetchQuestions = async () => {
    try {
      setIsLoading(true);
      const data = await supportService.getAllQuestions({ page, pageSize: 10, status, date });
      setQuestions(data.items || []);
      setTotalCount(data.totalCount || 0);
    } catch (error) {
      console.error('Failed to fetch questions:', error);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchQuestions();
  }, [page, status, date]);

  const handleStatusChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    setStatus(e.target.value);
    setPage(1);
  };

  const handleDateChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setDate(e.target.value);
    setPage(1);
  };

  const handleResetFilters = () => {
    setStatus('');
    setDate('');
    setPage(1);
  };

  const statusMap: Record<string, { label: string; bg: string; text: string }> = {
    New: { label: 'جديد', bg: 'bg-blue-100', text: 'text-blue-800' },
    Answered: { label: 'تمت الإجابة', bg: 'bg-green-100', text: 'text-green-800' },
    NoAnswer: { label: 'لا توجد إجابة', bg: 'bg-amber-100', text: 'text-amber-800' },
    Closed: { label: 'مغلق', bg: 'bg-gray-100', text: 'text-gray-800' }
  };

  const columns = [
    { 
      key: 'questionText', 
      header: 'نص السؤال',
      cell: (item: QuestionHistoryDto) => (
        <span className="font-medium text-gray-900 block max-w-md truncate" title={item.questionText}>
          {item.questionText}
        </span>
      )
    },
    { 
      key: 'employee', 
      header: 'الموظف',
      cell: (item: QuestionHistoryDto) => (
        <div className="flex items-center gap-2">
          <div className="w-7 h-7 rounded-full bg-blue-50 text-blue-600 font-bold text-xs flex items-center justify-center border border-blue-200 shrink-0">
            {item.employeeName ? item.employeeName.charAt(0).toUpperCase() : '؟'}
          </div>
          <div className="flex flex-col min-w-0">
            <span className="font-medium text-gray-900 text-sm truncate">{item.employeeName || 'غير محدد'}</span>
            {item.employeeEmail && (
              <span className="text-xs text-gray-400 truncate">{item.employeeEmail}</span>
            )}
          </div>
        </div>
      )
    },
    { 
      key: 'status', 
      header: 'الحالة',
      cell: (item: QuestionHistoryDto) => {
        const info = statusMap[item.status] || { label: item.status, bg: 'bg-gray-100', text: 'text-gray-800' };
        return (
          <span className={`px-2.5 py-1 rounded-full text-xs font-semibold ${info.bg} ${info.text}`}>
            {info.label}
          </span>
        );
      }
    },
    { key: 'createdAt', header: 'تاريخ الإنشاء', cell: (item: QuestionHistoryDto) => new Date(item.createdAt).toLocaleDateString('ar-EG') },
    { 
      key: 'actions', 
      header: 'الإجراءات', 
      cell: (item: QuestionHistoryDto) => (
        <Button onClick={() => setSelectedQuestion(item)} variant="outline" size="sm">التفاصيل</Button>
      )
    }
  ];

  return (
    <div className="p-6 rtl bg-gray-50 min-h-screen" dir="rtl">
      <div className="flex justify-between items-center mb-6">
        <h2 className="text-2xl font-bold text-gray-900">الأسئلة والاستفسارات</h2>
        <span className="text-sm text-gray-500 font-medium">إجمالي النتائج: {totalCount}</span>
      </div>

      <div className="flex flex-wrap items-end gap-4 mb-6 bg-white p-4 rounded-xl border border-gray-200 shadow-sm">
        <div className="w-64">
          <Select 
            options={[
              { label: 'جميع الحالات', value: '' },
              { label: 'جديد', value: 'New' },
              { label: 'تمت الإجابة', value: 'Answered' },
              { label: 'لا توجد إجابة', value: 'NoAnswer' },
              { label: 'مغلق', value: 'Closed' }
            ]}
            value={status}
            onChange={handleStatusChange}
            label="الحالة"
          />
        </div>
        <div className="w-64">
          <Input 
            type="date" 
            label="التاريخ" 
            value={date} 
            onChange={handleDateChange} 
          />
        </div>
        {(status !== '' || date !== '') && (
          <div className="pb-1">
            <Button 
              type="button" 
              variant="outline" 
              onClick={handleResetFilters}
              className="text-gray-600 hover:text-gray-900"
            >
              إعادة تعيين الفلاتر
            </Button>
          </div>
        )}
      </div>
      
      <DataTable 
        columns={columns} 
        data={questions} 
        currentPage={page} 
        totalPages={Math.max(1, Math.ceil(totalCount / 10))} 
        onPageChange={setPage} 
      />

      <Modal 
        isOpen={!!selectedQuestion} 
        onClose={() => setSelectedQuestion(null)} 
        title="تفاصيل السؤال"
      >
        {selectedQuestion && (
          <div className="space-y-4">
            <div>
              <strong className="block text-gray-700 mb-1">الموظف المُرسل:</strong>
              <div className="flex items-center gap-3 bg-gray-50 p-3 rounded-lg border border-gray-200">
                <div className="w-10 h-10 rounded-full bg-blue-100 text-blue-700 flex items-center justify-center font-bold text-sm">
                  {selectedQuestion.employeeName ? selectedQuestion.employeeName.charAt(0).toUpperCase() : '؟'}
                </div>
                <div>
                  <p className="font-semibold text-gray-900 text-sm">{selectedQuestion.employeeName || 'غير محدد'}</p>
                  <p className="text-xs text-gray-500 font-mono">{selectedQuestion.employeeEmail || selectedQuestion.employeeId || 'لا يوجد بريد إلكتروني'}</p>
                </div>
              </div>
            </div>
            <div>
              <strong className="block text-gray-700 mb-1">نص السؤال:</strong>
              <p className="text-gray-900 bg-gray-50 p-3 rounded-lg border border-gray-200 leading-relaxed">{selectedQuestion.questionText}</p>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <strong className="block text-gray-700 mb-1">الحالة:</strong>
                <span className={`inline-block px-2.5 py-1 rounded-full text-xs font-semibold ${statusMap[selectedQuestion.status]?.bg || 'bg-gray-100'} ${statusMap[selectedQuestion.status]?.text || 'text-gray-800'}`}>
                  {statusMap[selectedQuestion.status]?.label || selectedQuestion.status}
                </span>
              </div>
              <div>
                <strong className="block text-gray-700 mb-1">مجاب بالذكاء الاصطناعي:</strong>
                <span className={`inline-block px-2.5 py-1 rounded-full text-xs font-semibold ${selectedQuestion.answeredByAI ? 'bg-emerald-100 text-emerald-800' : 'bg-gray-100 text-gray-700'}`}>
                  {selectedQuestion.answeredByAI ? 'نعم' : 'لا'}
                </span>
              </div>
            </div>
            {selectedQuestion.confidenceScore !== undefined && selectedQuestion.confidenceScore !== null && (
              <div>
                <strong className="block text-gray-700 mb-1">نسبة الثقة:</strong>
                <p className="text-sm font-semibold text-gray-900">{(selectedQuestion.confidenceScore * 100).toFixed(1)}%</p>
              </div>
            )}
            <div>
              <strong className="block text-gray-700 mb-1">تاريخ ووقت الإنشاء:</strong>
              <p className="text-sm text-gray-600">{new Date(selectedQuestion.createdAt).toLocaleString('ar-EG')}</p>
            </div>
            <div className="pt-3 border-t border-gray-100 flex items-center justify-between text-xs text-gray-400">
              <span>الرقم المرجعي الفني:</span>
              <span className="font-mono text-gray-500 select-all">{selectedQuestion.id}</span>
            </div>
          </div>
        )}
      </Modal>
    </div>
  );
};
