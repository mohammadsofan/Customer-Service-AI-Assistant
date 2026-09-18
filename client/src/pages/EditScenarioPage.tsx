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
import { Plus, Trash2, Edit2, Check, X } from 'lucide-react';
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
  const [steps, setSteps] = useState<{ stepText: string; description?: string }[]>([]);
  const [stepInput, setStepInput] = useState('');
  const [stepDescriptionInput, setStepDescriptionInput] = useState('');
  const [editingStepIndex, setEditingStepIndex] = useState<number | null>(null);
  const [editingStepText, setEditingStepText] = useState('');
  const [editingStepDesc, setEditingStepDesc] = useState('');
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
      
      const loadedSteps = scenario.resolutionSteps?.map((s) => ({
        stepText: s.stepText,
        description: s.description || undefined
      })) || [];
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

  const handleEditStepStart = (index: number) => {
    const step = steps[index];
    setEditingStepIndex(index);
    setEditingStepText(step.stepText);
    setEditingStepDesc(step.description || '');
  };

  const handleEditStepCancel = () => {
    setEditingStepIndex(null);
    setEditingStepText('');
    setEditingStepDesc('');
  };

  const handleEditStepSave = () => {
    const trimmedText = editingStepText.trim();
    if (trimmedText && editingStepIndex !== null) {
      const newSteps = [...steps];
      newSteps[editingStepIndex] = {
        stepText: trimmedText,
        description: editingStepDesc.trim() || undefined
      };
      setSteps(newSteps);
      handleEditStepCancel();
    }
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
      await knowledgeService.updateScenario(id, {
        name: name.trim(),
        description: description.trim(),
        categoryId,
        keywords,
        resolutionSteps: validSteps.map(s => s.stepText),
        steps: validSteps,
      });

      if (status !== initialStatus) {
        await knowledgeService.updateStatus(id, isActive ? 'Active' : status);
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
            <p className="text-xs text-gray-400">لم يتم إضافة خطوات بعد.</p>
          ) : (
            <ol className="list-decimal list-inside space-y-2">
                {steps.map((step, index) => (
                  <li key={index} className="flex flex-col sm:flex-row justify-between items-start sm:items-center bg-gray-50 p-3 rounded-xl border border-slate-200">
                    {editingStepIndex === index ? (
                      <div className="flex-1 w-full space-y-3">
                        <Input
                          placeholder="تعديل الخطوة..."
                          value={editingStepText}
                          onChange={(e) => setEditingStepText(e.target.value)}
                        />
                        <Textarea
                          placeholder="تفاصيل إضافية (اختياري)..."
                          value={editingStepDesc}
                          onChange={(e) => setEditingStepDesc(e.target.value)}
                          rows={2}
                        />
                        <div className="flex justify-end gap-2 mt-2">
                          <button
                            type="button"
                            onClick={handleEditStepCancel}
                            className="flex items-center gap-1 px-3 py-1.5 text-sm bg-gray-200 text-gray-700 rounded-lg hover:bg-gray-300"
                          >
                            <X className="w-4 h-4" /> إلغاء
                          </button>
                          <button
                            type="button"
                            onClick={handleEditStepSave}
                            className="flex items-center gap-1 px-3 py-1.5 text-sm bg-[#76bc21] text-white rounded-lg hover:bg-[#67a61d]"
                          >
                            <Check className="w-4 h-4" /> حفظ
                          </button>
                        </div>
                      </div>
                    ) : (
                      <>
                        <div className="flex-1 ml-3 space-y-1">
                          <div className="font-semibold text-slate-800 text-sm">{step.stepText}</div>
                          {step.description && (
                            <div className="text-xs text-slate-500 bg-white p-2 rounded-lg border border-slate-200/80 inline-flex items-center gap-1.5 mt-1">
                              <span className="font-semibold text-blue-600">تفاصيل إضافية:</span>
                              <span className="text-slate-600">{step.description}</span>
                            </div>
                          )}
                        </div>
                        <div className="flex items-center gap-2 mt-2 sm:mt-0">
                          <button 
                            type="button" 
                            onClick={() => handleEditStepStart(index)} 
                            className="text-blue-500 hover:text-blue-700 p-1 cursor-pointer"
                            title="تعديل"
                          >
                            <Edit2 className="w-4 h-4" />
                          </button>
                          <button 
                            type="button" 
                            onClick={() => handleRemoveStep(index)} 
                            className="text-red-500 hover:text-red-700 p-1 cursor-pointer"
                            title="حذف"
                          >
                            <Trash2 className="w-4 h-4" />
                          </button>
                        </div>
                      </>
                    )}
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
