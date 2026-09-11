import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Button } from '../components/Button';
import { Input } from '../components/Input';
import { Select } from '../components/Select';
import { Textarea } from '../components/Textarea';
import knowledgeService, { Category } from '../services/knowledgeService';
import { Plus, Trash2 } from 'lucide-react';

export function CreateScenarioPage() {
  const navigate = useNavigate();
  const [categories, setCategories] = useState<Category[]>([]);
  
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [categoryId, setCategoryId] = useState('');
  const [keywords, setKeywords] = useState<string[]>([]);
  const [keywordInput, setKeywordInput] = useState('');
  const [steps, setSteps] = useState<string[]>([]);
  const [stepInput, setStepInput] = useState('');
  const [status, setStatus] = useState('active');
  const [isSaving, setIsSaving] = useState(false);

  useEffect(() => {
    knowledgeService.getCategories().then(setCategories).catch(console.error);
  }, []);

  const handleAddKeyword = () => {
    if (keywordInput.trim() && !keywords.includes(keywordInput.trim())) {
      setKeywords([...keywords, keywordInput.trim()]);
      setKeywordInput('');
    }
  };

  const handleRemoveKeyword = (kw: string) => {
    setKeywords(keywords.filter((k) => k !== kw));
  };

  const handleAddStep = () => {
    if (stepInput.trim()) {
      setSteps([...steps, stepInput.trim()]);
      setStepInput('');
    }
  };

  const handleRemoveStep = (index: number) => {
    setSteps(steps.filter((_, i) => i !== index));
  };

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!title || !description || !categoryId) {
      alert('الرجاء ملء الحقول المطلوبة (الاسم، الوصف، التصنيف)');
      return;
    }
    if (status === 'active' && steps.length === 0) {
      alert('الحالة النشطة تتطلب خطوة واحدة على الأقل');
      return;
    }

    try {
      setIsSaving(true);
      const content = JSON.stringify({ description, steps });
      await knowledgeService.createScenario({
        title,
        content,
        categoryId: Number(categoryId),
        keywords,
        // @ts-ignore
        status,
      });
      navigate('/admin/knowledge/scenarios');
    } catch (err) {
      console.error(err);
      alert('حدث خطأ أثناء الحفظ.');
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <div className="max-w-3xl mx-auto space-y-6" dir="rtl">
      <h1 className="text-2xl font-bold">إضافة سيناريو</h1>
      <form onSubmit={handleSave} className="space-y-6 bg-white p-6 rounded shadow">
        <Input
          label="الاسم (العنوان)"
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          required
        />
        <Textarea
          label="الوصف"
          value={description}
          onChange={(e) => setDescription(e.target.value)}
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
              onKeyPress={(e) => e.key === 'Enter' && (e.preventDefault(), handleAddKeyword())}
              placeholder="اكتب كلمة مفتاحية واضغط إضافة"
            />
            <Button type="button" onClick={handleAddKeyword} className="mt-1">إضافة</Button>
          </div>
          <div className="flex flex-wrap gap-2">
            {keywords.map((kw) => (
              <span key={kw} className="bg-blue-100 text-blue-800 px-2 py-1 rounded flex items-center gap-1">
                {kw}
                <button type="button" onClick={() => handleRemoveKeyword(kw)} className="text-red-500"><Trash2 className="w-3 h-3" /></button>
              </span>
            ))}
          </div>
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">خطوات الحل</label>
          <div className="flex gap-2 mb-2">
            <Input
              value={stepInput}
              onChange={(e) => setStepInput(e.target.value)}
              onKeyPress={(e) => e.key === 'Enter' && (e.preventDefault(), handleAddStep())}
              placeholder="اكتب خطوة واضغط إضافة"
            />
            <Button type="button" onClick={handleAddStep} className="mt-1">إضافة</Button>
          </div>
          <ol className="list-decimal list-inside space-y-2">
            {steps.map((step, index) => (
              <li key={index} className="flex justify-between items-center bg-gray-50 p-2 rounded">
                <span>{step}</span>
                <button type="button" onClick={() => handleRemoveStep(index)} className="text-red-500"><Trash2 className="w-4 h-4" /></button>
              </li>
            ))}
          </ol>
        </div>

        <Select
          label="الحالة"
          options={[
            { value: 'active', label: 'نشط' },
            { value: 'inactive', label: 'غير نشط' }
          ]}
          value={status}
          onChange={(e) => setStatus(e.target.value)}
        />

        <div className="flex justify-end gap-2 pt-4">
          <Button type="button" variant="outline" onClick={() => navigate('/admin/knowledge/scenarios')}>
            إلغاء
          </Button>
          <Button type="submit" isLoading={isSaving}>
            حفظ
          </Button>
        </div>
      </form>
    </div>
  );
}
