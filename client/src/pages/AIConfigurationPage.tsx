import React, { useState, useEffect } from 'react';
import { Button } from '../components/Button';
import { Select } from '../components/Select';
import { Input } from '../components/Input';
import { Textarea } from '../components/Textarea';
import { toast } from 'react-hot-toast';
import aiService, { type AiProvider, type AiModel, type AiConfiguration } from '../services/aiService';

export function AIConfigurationPage() {
  const [providers, setProviders] = useState<AiProvider[]>([]);
  const [models, setModels] = useState<AiModel[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [isTesting, setIsTesting] = useState(false);

  const [config, setConfig] = useState<AiConfiguration>({
    providerId: '',
    modelId: '',
    apiKey: '',
    isActive: true,
  });

  const [temperature, setTemperature] = useState('0.7');
  const [maxTokens, setMaxTokens] = useState('2000');
  const [similarityThreshold, setSimilarityThreshold] = useState('0.85');
  const [topK, setTopK] = useState('3');
  const [systemPrompt, setSystemPrompt] = useState('أنت مساعد ذكي لخدمة العملاء...');
  const [autoFailover, setAutoFailover] = useState(true);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      setIsLoading(true);
      const [provs, configuration] = await Promise.all([
        aiService.getProviders(),
        aiService.getConfiguration().catch(() => null)
      ]);
      setProviders(provs);
      
      if (configuration) {
        setConfig(configuration);
        if (configuration.providerId) {
          const provModels = await aiService.getModels(configuration.providerId);
          setModels(provModels);
        }
      }
    } catch (error) {
      toast.error('حدث خطأ أثناء تحميل الإعدادات');
    } finally {
      setIsLoading(false);
    }
  };

  const handleProviderChange = async (providerId: string) => {
    setConfig({ ...config, providerId, modelId: '' });
    try {
      if (providerId) {
        const provModels = await aiService.getModels(providerId);
        setModels(provModels);
      } else {
        setModels([]);
      }
    } catch {
      toast.error('خطأ في تحميل النماذج');
    }
  };

  const handleSave = async () => {
    try {
      setIsSaving(true);
      await aiService.saveConfiguration(config);
      toast.success('تم حفظ الإعدادات بنجاح');
    } catch {
      toast.error('حدث خطأ أثناء الحفظ');
    } finally {
      setIsSaving(false);
    }
  };

  const handleTestConnection = async () => {
    try {
      setIsTesting(true);
      const success = await aiService.testConnection(config);
      if (success) {
        toast.success('تم الاتصال بنجاح');
      } else {
        toast.error('فشل الاتصال بالمزود');
      }
    } catch {
      toast.error('فشل الاتصال بالمزود');
    } finally {
      setIsTesting(false);
    }
  };

  if (isLoading) {
    return <div className="p-6" dir="rtl">جاري التحميل...</div>;
  }

  return (
    <div className="p-6 max-w-4xl mx-auto" dir="rtl">
      <h1 className="text-2xl font-bold text-gray-900 mb-6">إعدادات الذكاء الاصطناعي العامة</h1>
      
      <div className="bg-white rounded-lg shadow border border-gray-200 p-6 space-y-6">
        
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <Select
            label="المزود النشط"
            value={config.providerId}
            onChange={(e) => handleProviderChange(e.target.value)}
            options={[{ value: '', label: 'اختر المزود' }, ...providers.map(p => ({ value: p.id, label: p.name }))]}
          />
          <Select
            label="النموذج النشط"
            value={config.modelId}
            onChange={(e) => setConfig({ ...config, modelId: e.target.value })}
            options={[{ value: '', label: 'اختر النموذج' }, ...models.map(m => ({ value: m.id, label: m.name }))]}
            disabled={!config.providerId}
          />
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <Input
            label="درجة الإبداع (Temperature)"
            type="number"
            step="0.1"
            min="0"
            max="2"
            value={temperature}
            onChange={(e) => setTemperature(e.target.value)}
          />
          <Input
            label="الحد الأقصى للكلمات (Max Tokens)"
            type="number"
            value={maxTokens}
            onChange={(e) => setMaxTokens(e.target.value)}
          />
          <Input
            label="عتبة التشابه (Similarity Threshold)"
            type="number"
            step="0.05"
            min="0"
            max="1"
            value={similarityThreshold}
            onChange={(e) => setSimilarityThreshold(e.target.value)}
          />
          <Input
            label="أفضل النتائج (Top K)"
            type="number"
            value={topK}
            onChange={(e) => setTopK(e.target.value)}
          />
        </div>

        <div>
          <Textarea
            label="موجه النظام (System Prompt)"
            value={systemPrompt}
            onChange={(e) => setSystemPrompt(e.target.value)}
            rows={4}
          />
        </div>

        <div className="flex items-center gap-3">
          <input
            type="checkbox"
            id="autoFailover"
            checked={autoFailover}
            onChange={(e) => setAutoFailover(e.target.checked)}
            className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
          />
          <label htmlFor="autoFailover" className="text-sm font-medium text-gray-700">
            تفعيل الانتقال التلقائي لمزود احتياطي عند الفشل (Auto Failover)
          </label>
        </div>

        <div className="pt-4 border-t border-gray-200 flex gap-4">
          <Button onClick={handleSave} isLoading={isSaving}>
            حفظ الإعدادات
          </Button>
          <Button variant="outline" onClick={handleTestConnection} isLoading={isTesting}>
            اختبار الاتصال
          </Button>
        </div>
      </div>
    </div>
  );
}
