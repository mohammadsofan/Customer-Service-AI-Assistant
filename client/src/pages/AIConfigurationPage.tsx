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
  const [embeddingModels, setEmbeddingModels] = useState<AiModel[]>([]);
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
        const activeProvId = configuration.activeProviderId || configuration.providerId || '';
        const activeModId = configuration.activeModelId || configuration.modelId || '';
        setConfig({
          ...configuration,
          providerId: activeProvId,
          activeProviderId: activeProvId,
          modelId: activeModId,
          activeModelId: activeModId,
          activeEmbeddingProviderId: configuration.activeEmbeddingProviderId || '',
          activeEmbeddingModelId: configuration.activeEmbeddingModelId || ''
        });
        if (configuration.temperature !== undefined) setTemperature(configuration.temperature.toString());
        if (configuration.maxTokens !== undefined) setMaxTokens(configuration.maxTokens.toString());
        if (configuration.similarityThreshold !== undefined) setSimilarityThreshold(configuration.similarityThreshold.toString());
        if (configuration.topK !== undefined) setTopK(configuration.topK.toString());
        if (configuration.systemPrompt) setSystemPrompt(configuration.systemPrompt);
        if (configuration.enableAutoFailover !== undefined) setAutoFailover(configuration.enableAutoFailover);

        if (activeProvId) {
          const provModels = await aiService.getModels(activeProvId);
          setModels(provModels.filter(m => !m.isEmbeddingModel));
        }
        if (configuration.activeEmbeddingProviderId) {
          const embModels = await aiService.getModels(configuration.activeEmbeddingProviderId);
          setEmbeddingModels(embModels.filter(m => m.isEmbeddingModel));
        }
      }
    } catch (error) {
      toast.error('حدث خطأ أثناء تحميل الإعدادات');
    } finally {
      setIsLoading(false);
    }
  };

  const handleProviderChange = async (providerId: string) => {
    setConfig({ ...config, providerId, activeProviderId: providerId, modelId: '', activeModelId: '' });
    try {
      if (providerId) {
        const provModels = await aiService.getModels(providerId);
        setModels(provModels.filter(m => !m.isEmbeddingModel));
      } else {
        setModels([]);
      }
    } catch {
      toast.error('خطأ في تحميل النماذج');
    }
  };

  const handleSave = async () => {
    const activeProvId = config.activeProviderId || config.providerId;
    const activeModId = config.activeModelId || config.modelId;
    if (!activeProvId) {
      toast.error('يرجى اختيار المزود النشط');
      return;
    }
    if (!activeModId) {
      toast.error('يرجى اختيار النموذج النشط');
      return;
    }

    try {
      setIsSaving(true);
      await aiService.saveConfiguration({
        activeProviderId: activeProvId,
        activeModelId: activeModId,
        temperature: parseFloat(temperature) || 0.7,
        maxTokens: parseInt(maxTokens) || 1024,
        similarityThreshold: parseFloat(similarityThreshold) || 0.75,
        topK: parseInt(topK) || 5,
        systemPrompt,
        enableAutoFailover: autoFailover
      });
      toast.success('تم حفظ الإعدادات بنجاح');
    } catch {
      toast.error('حدث خطأ أثناء الحفظ');
    } finally {
      setIsSaving(false);
    }
  };

  const handleTestConnection = async () => {
    const activeProvId = config.activeProviderId || config.providerId;
    if (!activeProvId) {
      toast.error('يرجى اختيار المزود أولاً لاختبار الاتصال');
      return;
    }
    try {
      setIsTesting(true);
      const res = await aiService.testConnection({ activeProviderId: activeProvId });
      if (res.success) {
        toast.success(res.message || 'تم الاتصال بنجاح بالمزود');
      } else {
        toast.error(res.message || 'فشل الاتصال بالمزود', { duration: 7000 });
      }
    } catch (err: any) {
      toast.error(err?.message || 'فشل الاتصال بالمزود');
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
            label="المزود النشط (للإجابات)"
            value={config.activeProviderId || config.providerId || ''}
            onChange={(e) => handleProviderChange(e.target.value)}
            options={[{ value: '', label: 'اختر المزود' }, ...providers.map(p => ({ value: p.id, label: p.name }))]}
          />
          <Select
            label="النموذج النشط (للإجابات)"
            value={config.activeModelId || config.modelId || ''}
            onChange={(e) => setConfig({ ...config, modelId: e.target.value, activeModelId: e.target.value })}
            options={[{ value: '', label: 'اختر النموذج' }, ...models.map(m => ({ value: m.id, label: m.name || m.modelName || 'Model' }))]}
            disabled={!(config.activeProviderId || config.providerId)}
          />
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <Select
            label="مزود التضمين والبحث (Embedding)"
            value={config.activeEmbeddingProviderId || ''}
            onChange={async (e) => {
              const pId = e.target.value;
              setConfig({ ...config, activeEmbeddingProviderId: pId, activeEmbeddingModelId: '' });
              if (pId) {
                try {
                  const embModels = await aiService.getModels(pId);
                  setEmbeddingModels(embModels.filter(m => m.isEmbeddingModel));
                } catch {
                  toast.error('خطأ في تحميل نماذج التضمين');
                }
              } else {
                setEmbeddingModels([]);
              }
            }}
            options={[{ value: '', label: 'بدون (استخدام الإعدادات الافتراضية)' }, ...providers.map(p => ({ value: p.id, label: p.name }))]}
          />
          <Select
            label="نموذج التضمين (Embedding)"
            value={config.activeEmbeddingModelId || ''}
            onChange={(e) => setConfig({ ...config, activeEmbeddingModelId: e.target.value })}
            options={[{ value: '', label: 'اختر النموذج' }, ...embeddingModels.map(m => ({ value: m.id, label: m.name || m.modelName || 'Model' }))]}
            disabled={!config.activeEmbeddingProviderId}
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
