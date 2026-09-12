import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { Button } from '../components/Button';
import { Input } from '../components/Input';
import { Select } from '../components/Select';
import { Textarea } from '../components/Textarea';
import { Alert } from '../components/Alert';
import { ErrorDialog } from '../components/ErrorDialog';
import { LoadingState } from '../components/LoadingState';
import knowledgeService, { Category, Scenario } from '../services/knowledgeService';
import { Plus, Trash2 } from 'lucide-react';
import toast from 'react-hot-toast';

export function EditScenarioPage() {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const [loading, setLoading] = useState(true);
  const [categories, setCategories] = useState<Category[]>([]);
  
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [categoryId, setCategoryId] = useState('');
  const [keywords, setKeywords] = useState<string[]>([]);
  const [keywordInput, setKeywordInput] = useState('');
  const [steps, setSteps] = useState<string[]>([]);
  const [stepInput, setStepInput] = useState('');
  const [status, setStatus] = useState('Active');
  const [initialStatus, setInitialStatus] = useState('Active');
  const [isSaving, setIsSaving] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const [errorDialog, setErrorDialog] = useState<{
    isOpen: boolean;
    title?: string;
    message: string;
    errors?: string[];
  }>({
    isOpen: false,
    message: '',
  });

  const showError = (message: string, errors?: string[], title?: string) => {
    setErrorDialog({
      isOpen: true,
      title: title || 'حدث خطأ أثناء الحفظ',
      message,
      errors,
    });
    setErrorMessage(message);
    toast.error(message);
  };

  useEffect(() => {
    if (!id) return;
    Promise.all([
      knowledgeService.getScenario(id),
      knowledgeService.getCategories()
    ]).then(([scenario, cats]) => {
      setCategories(cats);
      setName(scenario.name || '');
      setDescription(scenario.description || '');
      setCategoryId(scenario.categoryId?.toString() || '');
      setKeywords(scenario.keywords || []);
      const scenarioStatus = scenario.status || 'Active';
      setStatus(scenarioStatus);
      setInitialStatus(scenarioStatus);
      
      const loadedSteps = scenario.resolutionSteps?.map((s) => s.stepText) || [];
      setSteps(loadedSteps);
      setLoading(false);
    }).catch((err) => {
      console.error(err);
      setErrorMessage('فشل في تحميل بيانات السيناريو.');
      setLoading(false);
    });
  }, [id]);

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
    if (!id) return;
    setErrorMessage(null);

    if (!name.trim() || !description.trim() || !categoryId) {
      const missingFields: string[] = [];
      if (!name.trim()) missingFields.push('اسم السيناريو مطلوب');
      if (!description.trim()) missingFields.push('وصف السيناريو مطلوب');
      if (!categoryId) missingFields.push('يرجى اختيار تصنيف للسيناريو');
      showError('الرجاء استكمال جميع الحقول الإلزامية قبل حفظ السيناريو.', missingFields, 'تنبيه: بيانات غير مكتملة');
      return;
    }
    if (status === 'Active' && steps.length === 0) {
      showError('تفعيل السيناريو يتطلب إضافة خطوة حل واحدة على الأقل لتمكين الذكاء الاصطناعي من الإجابة.', ['أضف خطوات حل مرتبة أو اضبط الحالة إلى "مسودة"'], 'تنبيه: خطوات الحل مطلوبة');
      return;
    }

    try {
      setIsSaving(true);
      await knowledgeService.updateScenario(id, {
        name: name.trim(),
        description: description.trim(),
        categoryId,
        keywords,
        resolutionSteps: steps,
      });

      if (status !== initialStatus) {
        await knowledgeService.updateStatus(id, status);
      }

      toast.success('تم تحديث السيناريو بنجاح');
      navigate('/admin/knowledge');
    } catch (err: any) {
      console.error('Error updating scenario:', err);
      let errorList: string[] = [];
      let mainMsg = 'تعذر تحديث السيناريو في النظام.';

      if (err.response?.data?.errors) {
        errorList = Object.values(err.response.data.errors).flat() as string[];
        mainMsg = 'يرجى مراجعة وتصحيح المدخلات التالية والمحاولة مرة أخرى.';
      } else if (err.response?.data?.message) {
        mainMsg = err.response.data.message;
      } else if (err.response?.data?.title) {
        mainMsg = err.response.data.title;
      } else if (err.message) {
        mainMsg = err.message;
      }

      showError(mainMsg, errorList.length > 0 ? errorList : undefined, 'فشل تحديث السيناريو');
    } finally {
      setIsSaving(false);
    }
  };

  if (loading) return <LoadingState />;

  return (
    <div className="max-w-3xl mx-auto space-y-6" dir="rtl">
      <h1 className="text-2xl font-bold">تعديل السيناريو</h1>

      <ErrorDialog
        isOpen={errorDialog.isOpen}
        onClose={() => setErrorDialog((prev) => ({ ...prev, isOpen: false }))}
        title={errorDialog.title}
        message={errorDialog.message}
        errors={errorDialog.errors}
      />

      {errorMessage && (
        <Alert
          type="error"
          title="خطأ"
          message={errorMessage}
        />
      )}

      <form onSubmit={handleSave} className="space-y-6 bg-white p-6 rounded shadow">
        <Input
          label="اسم السيناريو"
          value={name}
          onChange={(e) => setName(e.target.value)}
          required
        />

        <Textarea
          label="الوصف"
          value={description}
          onChange={(e) => setDescription(e.target.value)}
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
              placeholder="اكتب خطوة واضغط إضافة"
            />
            <Button type="button" onClick={handleAddStep} className="mt-1">
              <Plus className="w-4 h-4 ml-1" />
              إضافة
            </Button>
          </div>
          {steps.length === 0 ? (
            <p className="text-xs text-gray-400">لم يتم إضافة خطوات بعد.</p>
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
            حفظ التعديلات
          </Button>
        </div>
      </form>
    </div>
  );
}
