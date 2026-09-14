import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Button } from '../components/Button';
import { Input } from '../components/Input';
import { Select } from '../components/Select';
import { Textarea } from '../components/Textarea';
import { Alert } from '../components/Alert';
import { ErrorDialog } from '../components/ErrorDialog';
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
  const [steps, setSteps] = useState<{ stepText: string; description?: string }[]>([]);
  const [stepInput, setStepInput] = useState('');
  const [stepDescriptionInput, setStepDescriptionInput] = useState('');
  const [status, setStatus] = useState('Active');
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
      setSteps([...steps, {
        stepText: trimmed,
        description: stepDescriptionInput.trim() || undefined
      }]);
      setStepInput('');
      setStepDescriptionInput('');
    }
  };

  const handleRemoveStep = (index: number) => {
    setSteps(steps.filter((_, i) => i !== index));
  };

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrorMessage(null);

    if (!name.trim() || !description.trim() || !categoryId) {
      const missingFields: string[] = [];
      if (!name.trim()) missingFields.push('اسم السيناريو مطلوب');
      if (!description.trim()) missingFields.push('وصف السيناريو مطلوب');
      if (!categoryId) missingFields.push('يرجى اختيار تصنيف للسيناريو');
      showError('الرجاء استكمال جميع الحقول الإلزامية قبل حفظ السيناريو.', missingFields, 'تنبيه: بيانات غير مكتملة');
      return;
    }

    const isScenarioActive = (val?: string) => {
      if (!val) return true;
      const s = val.trim().toLowerCase();
      return s === 'active' || s === 'نشط' || s === '1' || (s !== 'draft' && s !== 'inactive' && s !== 'archived' && s !== 'مسودة' && s !== 'غير نشط');
    };

    const isActive = isScenarioActive(status);
    const validSteps = steps.filter((s) => s.stepText && s.stepText.trim().length > 0);

    if (isActive && validSteps.length === 0) {
      showError('تفعيل السيناريو يتطلب إضافة خطوة حل واحدة على الأقل لتمكين الذكاء الاصطناعي من الإجابة.', ['أضف خطوات حل مرتبة أو اضبط الحالة إلى "مسودة"'], 'تنبيه: خطوات الحل مطلوبة');
      return;
    }

    try {
      setIsSaving(true);
      await knowledgeService.createScenario({
        name: name.trim(),
        description: description.trim(),
        categoryId,
        keywords,
        resolutionSteps: validSteps.map(s => s.stepText),
        steps: validSteps,
        status: isActive ? 'Active' : status,
      });

      toast.success('تمت إضافة السيناريو بنجاح');
      navigate('/admin/knowledge');
    } catch (err: any) {
      console.error('Error creating scenario:', err);
      let errorList: string[] = [];
      let mainMsg = 'تعذر حفظ السيناريو في النظام.';

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

      showError(mainMsg, errorList.length > 0 ? errorList : undefined, 'فشل حفظ السيناريو');
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <div className="max-w-3xl mx-auto space-y-6" dir="rtl">
      <h1 className="text-2xl font-bold">إضافة سيناريو جديد</h1>

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
          title="خطأ في الحفظ"
          message={errorMessage}
        />
      )}

      <form onSubmit={handleSave} className="space-y-6 bg-white p-6 rounded shadow">
        <Input
          label="اسم السيناريو"
          name="name"
          id="scenario-name"
          data-testid="scenario-name-input"
          value={name}
          onChange={(e) => setName(e.target.value)}
          placeholder="مثال: مشكلة عدم وصول رمز التحقق OTP"
          required
        />

        <Textarea
          label="الوصف"
          name="description"
          id="scenario-description"
          data-testid="scenario-description-input"
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          placeholder="اكتب وصفاً مفصلاً للمشكلة وسياق حدوثها"
          rows={4}
          required
        />

        <Select
          label="التصنيف"
          name="categoryId"
          id="scenario-category"
          data-testid="scenario-category-select"
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
              name="keywordInput"
              data-testid="keyword-input"
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
            <Button type="button" onClick={handleAddKeyword} className="mt-1" data-testid="add-keyword-btn">
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
          <div className="space-y-2 mb-3 bg-slate-50/70 p-3.5 rounded-xl border border-slate-200">
            <Input
              name="stepInput"
              data-testid="step-input"
              value={stepInput}
              onChange={(e) => setStepInput(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === 'Enter') {
                  e.preventDefault();
                  handleAddStep();
                }
              }}
              placeholder="اكتب عنوان أو نص خطوة الحل الرئيسية..."
            />
            <Textarea
              rows={2}
              value={stepDescriptionInput}
              onChange={(e) => setStepDescriptionInput(e.target.value)}
              placeholder="شرح أو تفاصيل إضافية اختيارية للخطوة (تظهر للموظف بنافذة منبثقة عند النقر عليها)"
            />
            <div className="flex justify-start">
              <Button type="button" onClick={handleAddStep} data-testid="add-step-btn" size="sm">
                <Plus className="w-4 h-4 ml-1" />
                إضافة الخطوة
              </Button>
            </div>
          </div>
          {steps.length === 0 ? (
            <p className="text-xs text-gray-400">لم يتم إضافة خطوات بعد. أضف خطوات مرتبة لمساعدة الذكاء الاصطناعي على حل المشكلة.</p>
          ) : (
            <ol className="list-decimal list-inside space-y-2">
              {steps.map((step, index) => (
                <li key={index} className="flex justify-between items-start bg-gray-50 p-3 rounded-xl border border-slate-200">
                  <div className="flex-1 ml-3 space-y-1">
                    <div className="font-semibold text-slate-800 text-sm">{step.stepText}</div>
                    {step.description && (
                      <div className="text-xs text-slate-500 bg-white p-2 rounded-lg border border-slate-200/80 inline-flex items-center gap-1.5 mt-1">
                        <span className="font-semibold text-blue-600">تفاصيل إضافية:</span>
                        <span className="text-slate-600">{step.description}</span>
                      </div>
                    )}
                  </div>
                  <button type="button" onClick={() => handleRemoveStep(index)} className="text-red-500 hover:text-red-700 mt-1 cursor-pointer">
                    <Trash2 className="w-4 h-4" />
                  </button>
                </li>
              ))}
            </ol>
          )}
        </div>

        <Select
          label="الحالة"
          name="status"
          id="scenario-status"
          data-testid="scenario-status-select"
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
          <Button type="submit" isLoading={isSaving} data-testid="submit-scenario-btn">
            حفظ السيناريو
          </Button>
        </div>
      </form>
    </div>
  );
}
