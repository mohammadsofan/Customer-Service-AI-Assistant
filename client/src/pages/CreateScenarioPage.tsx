import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Button } from '../components/Button';
import { Input } from '../components/Input';
import { Select } from '../components/Select';
import { Textarea } from '../components/Textarea';
import { Alert } from '../components/Alert';
import knowledgeService, { Category } from '../services/knowledgeService';
import { Plus, Trash2 } from 'lucide-react';
import toast from 'react-hot-toast';

export function CreateScenarioPage() {
  const navigate = useNavigate();
  const [categories, setCategories] = useState<Category[]>([]);
  
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [categoryId, setCategoryId] = useState('');
  const [keywords, setKeywords] = useState<string[]>([]);
  const [keywordInput, setKeywordInput] = useState('');
  const [steps, setSteps] = useState<string[]>([]);
  const [stepInput, setStepInput] = useState('');
  const [status, setStatus] = useState('Active');
  const [isSaving, setIsSaving] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  useEffect(() => {
    knowledgeService.getCategories().then(setCategories).catch(console.error);
  }, []);

  const handleAddKeyword = () => {
    const trimmed = keywordInput.trim();
    if (trimmed && !keywords.includes(trimmed)) {
      setKeywords([...keywords, trimmed]);
      setKeywordInput('');
    }
  };

  const handleRemoveKeyword = (kw: string) => {
    setKeywords(keywords.filter((k) => k !== kw));
  };

  const handleAddStep = () => {
    const trimmed = stepInput.trim();
    if (trimmed) {
      setSteps([...steps, trimmed]);
      setStepInput('');
    }
  };

  const handleRemoveStep = (index: number) => {
    setSteps(steps.filter((_, i) => i !== index));
  };

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrorMessage(null);

    if (!name.trim() || !description.trim() || !categoryId) {
      setErrorMessage('الرجاء ملء الحقول المطلوبة (اسم السيناريو، الوصف، التصنيف)');
      return;
    }

    if (status === 'Active' && steps.length === 0) {
      setErrorMessage('تفعيل السيناريو يتطلب إضافة خطوة حل واحدة على الأقل');
      return;
    }

    try {
      setIsSaving(true);
      await knowledgeService.createScenario({
        name: name.trim(),
        description: description.trim(),
        categoryId,
        keywords,
        resolutionSteps: steps,
        status,
      });

      toast.success('تمت إضافة السيناريو بنجاح');
      navigate('/admin/knowledge');
    } catch (err: any) {
      console.error('Error creating scenario:', err);
      let msg = 'حدث خطأ أثناء الحفظ.';
      if (err.response?.data?.errors) {
        msg = Object.values(err.response.data.errors).flat().join(' | ');
      } else if (err.response?.data?.message) {
        msg = err.response.data.message;
      } else if (err.response?.data?.title) {
        msg = err.response.data.title;
      } else if (err.message) {
        msg = err.message;
      }
      setErrorMessage(msg);
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <div className="max-w-3xl mx-auto space-y-6" dir="rtl">
      <h1 className="text-2xl font-bold">إضافة سيناريو جديد</h1>

      {errorMessage && (
        <Alert
          type="error"
          title="خطأ في الحفظ"
          message={errorMessage}
        />
      )}

      <form onSubmit={handleSave} className="space-y-6 bg-white p-6 rounded shadow">
        <Input
          label="اسم السيناريو"
          value={name}
          onChange={(e) => setName(e.target.value)}
          placeholder="مثال: مشكلة عدم وصول رمز التحقق OTP"
          required
        />

        <Textarea
          label="الوصف"
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          placeholder="اكتب وصفاً مفصلاً للمشكلة وسياق حدوثها"
          rows={4}
          required
        />

        <Select
          label="التصنيف"
          options={[
            { value: '', label: 'اختر تصنيفاً' },
            ...categories.map((c) => ({ value: c.id.toString(), label: c.name }))
          ]}
          value={categoryId}
          onChange={(e) => setCategoryId(e.target.value)}
          required
        />
        
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">الكلمات المفتاحية</label>
          <div className="flex gap-2 mb-2">
            <Input
              value={keywordInput}
              onChange={(e) => setKeywordInput(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === 'Enter') {
                  e.preventDefault();
                  handleAddKeyword();
                }
              }}
              placeholder="اكتب كلمة مفتاحية واضغط إضافة"
            />
            <Button type="button" onClick={handleAddKeyword} className="mt-1">
              <Plus className="w-4 h-4 ml-1" />
              إضافة
            </Button>
          </div>
          <div className="flex flex-wrap gap-2">
            {keywords.map((kw) => (
              <span key={kw} className="bg-blue-100 text-blue-800 px-3 py-1 rounded-full text-sm flex items-center gap-1">
                {kw}
                <button type="button" onClick={() => handleRemoveKeyword(kw)} className="text-red-500 hover:text-red-700 mr-1">
                  <Trash2 className="w-3.5 h-3.5" />
                </button>
              </span>
            ))}
          </div>
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">خطوات الحل (المرتبة)</label>
          <div className="flex gap-2 mb-2">
            <Input
              value={stepInput}
              onChange={(e) => setStepInput(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === 'Enter') {
                  e.preventDefault();
                  handleAddStep();
                }
              }}
              placeholder="اكتب خطوة من خطوات الحل واضغط إضافة"
            />
            <Button type="button" onClick={handleAddStep} className="mt-1">
              <Plus className="w-4 h-4 ml-1" />
              إضافة
            </Button>
          </div>
          {steps.length === 0 ? (
            <p className="text-xs text-gray-400">لم يتم إضافة خطوات بعد. أضف خطوات مرتبة لمساعدة الذكاء الاصطناعي على حل المشكلة.</p>
          ) : (
            <ol className="list-decimal list-inside space-y-2">
              {steps.map((step, index) => (
                <li key={index} className="flex justify-between items-center bg-gray-50 p-3 rounded border">
                  <span>{step}</span>
                  <button type="button" onClick={() => handleRemoveStep(index)} className="text-red-500 hover:text-red-700">
                    <Trash2 className="w-4 h-4" />
                  </button>
                </li>
              ))}
            </ol>
          )}
        </div>

        <Select
          label="الحالة"
          options={[
            { value: 'Active', label: 'نشط' },
            { value: 'Draft', label: 'مسودة' },
            { value: 'Inactive', label: 'غير نشط' }
          ]}
          value={status}
          onChange={(e) => setStatus(e.target.value)}
        />

        <div className="flex justify-end gap-2 pt-4 border-t">
          <Button type="button" variant="outline" onClick={() => navigate('/admin/knowledge')}>
            إلغاء
          </Button>
          <Button type="submit" isLoading={isSaving}>
            حفظ السيناريو
          </Button>
        </div>
      </form>
    </div>
  );
}
