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
  const [selectedQuestion, setSelectedQuestion] = useState<QuestionHistoryDto | null>(null);

  const fetchQuestions = async () => {
    try {
      const data = await supportService.getAllQuestions({ page, pageSize: 10, status, date });
      setQuestions(data.items);
      setTotalCount(data.totalCount);
    } catch (error) {
      console.error('Failed to fetch questions:', error);
    }
  };

  useEffect(() => {
    fetchQuestions();
  }, [page, status, date]);

  const columns = [
    { key: 'id', header: 'معرف السؤال' },
    { key: 'questionText', header: 'النص' },
    { key: 'status', header: 'الحالة' },
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
      <h2 className="text-2xl font-bold mb-6">الأسئلة</h2>
      <div className="flex gap-4 mb-6">
        <div className="w-1/3">
          <Select 
            options={[
              { label: 'الكل', value: '' },
              { label: 'جديد', value: 'New' },
              { label: 'تمت الإجابة', value: 'Answered' },
              { label: 'لا توجد إجابة', value: 'NoAnswer' },
              { label: 'مغلق', value: 'Closed' }
            ]}
            value={status}
            onChange={(e) => setStatus(e.target.value)}
            label="الحالة"
          />
        </div>
        <div className="w-1/3">
          <Input 
            type="date" 
            label="التاريخ" 
            value={date} 
            onChange={(e) => setDate(e.target.value)} 
          />
        </div>
      </div>
      
      <DataTable 
        columns={columns} 
        data={questions} 
        currentPage={page} 
        totalPages={Math.ceil(totalCount / 10)} 
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
              <strong className="block text-gray-700">معرف السؤال:</strong>
              <p>{selectedQuestion.id}</p>
            </div>
            <div>
              <strong className="block text-gray-700">النص:</strong>
              <p>{selectedQuestion.questionText}</p>
            </div>
            <div>
              <strong className="block text-gray-700">الحالة:</strong>
              <p>{selectedQuestion.status}</p>
            </div>
            <div>
              <strong className="block text-gray-700">مجاب بواسطة الذكاء الاصطناعي:</strong>
              <p>{selectedQuestion.answeredByAI ? 'نعم' : 'لا'}</p>
            </div>
            {selectedQuestion.confidenceScore !== undefined && (
              <div>
                <strong className="block text-gray-700">نسبة الثقة:</strong>
                <p>{(selectedQuestion.confidenceScore * 100).toFixed(1)}%</p>
              </div>
            )}
          </div>
        )}
      </Modal>
    </div>
  );
};
