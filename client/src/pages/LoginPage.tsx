import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';
import { Button } from '../components/Button';
import { Input } from '../components/Input';
import { Alert } from '../components/Alert';
import { Bot, Sparkles, Shield, User as UserIcon, Lock, ArrowLeft } from 'lucide-react';

export const LoginPage: React.FC = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (!email || !password) {
      setError('يرجى إدخال البريد الإلكتروني وكلمة المرور.');
      return;
    }

    try {
      setIsLoading(true);
      const loggedInUser = await login({ email, password });
      const roleLower = (loggedInUser?.role || '').toLowerCase();
      const isAdmin = loggedInUser?.isAdmin || roleLower === 'admin' || roleLower === 'administrator';
      if (isAdmin) {
        navigate('/admin', { replace: true });
      } else {
        navigate('/support', { replace: true });
      }
    } catch (err: any) {
      if (
        err.response?.status === 401 ||
        err.response?.status === 403 ||
        err.response?.data?.errorCode === 'UNAUTHORIZED' ||
        err.response?.data?.message?.toLowerCase().includes('authorized') ||
        err.response?.data?.message?.toLowerCase().includes('credential') ||
        err.response?.data?.message?.includes('بيانات الاعتماد')
      ) {
        setError('بيانات الاعتماد غير صحيحة. يرجى التأكد من البريد وكلمة المرور.');
      } else {
        setError(err.response?.data?.message || 'بيانات الاعتماد غير صحيحة. يرجى التأكد من البريد وكلمة المرور.');
      }
      setIsLoading(false);
    }
  };

  const fillCredentials = (eMail: string, pass: string) => {
    setEmail(eMail);
    setPassword(pass);
    setError('');
  };

  return (
    <div className="bg-white/95 backdrop-blur-xl rounded-2xl shadow-2xl border border-white/20 p-8 sm:p-10 transition-all duration-300">
      {/* Header & Logo */}
      <div className="text-center space-y-3 mb-8">
        <div className="inline-flex items-center justify-center w-16 h-16 rounded-2xl bg-gradient-to-tr from-blue-600 to-indigo-500 shadow-lg shadow-blue-500/30 text-white mb-2">
          <Bot className="w-9 h-9" />
        </div>
        <div className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full bg-blue-50 border border-blue-200/60 text-xs font-semibold text-blue-700">
          <Sparkles className="w-3.5 h-3.5 text-blue-600" />
          <span>مساعد خدمة العملاء الذكي</span>
        </div>
        <h1 className="text-2xl sm:text-3xl font-bold tracking-tight text-slate-900">
          تسجيل الدخول
        </h1>
        <p className="text-sm text-slate-500">
          أدخل بيانات حسابك للمتابعة إلى بوابة الدعم الذكية
        </p>
      </div>

      {/* Error Alert */}
      {error && (
        <div className="mb-6">
          <Alert type="error" message={error} />
        </div>
      )}

      {/* Form */}
      <form className="space-y-5" onSubmit={handleSubmit}>
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1.5">
              البريد الإلكتروني
            </label>
            <div className="relative">
              <input
                type="email"
                required
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="أدخل بريدك الإلكتروني"
                disabled={isLoading}
                dir="rtl"
                className="w-full px-4 py-3 bg-slate-50/70 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all text-slate-900 placeholder:text-slate-400 text-sm"
              />
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1.5">
              كلمة المرور
            </label>
            <div className="relative">
              <input
                type="password"
                required
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="أدخل كلمة المرور"
                disabled={isLoading}
                dir="rtl"
                className="w-full px-4 py-3 bg-slate-50/70 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all text-slate-900 placeholder:text-slate-400 text-sm"
              />
            </div>
          </div>
        </div>

        <button
          type="submit"
          disabled={isLoading}
          className="w-full py-3.5 px-4 rounded-xl bg-gradient-to-r from-blue-600 to-indigo-600 hover:from-blue-700 hover:to-indigo-700 text-white font-semibold shadow-lg shadow-blue-500/25 hover:shadow-blue-500/40 active:scale-[0.99] transition-all duration-200 flex items-center justify-center gap-2 cursor-pointer disabled:opacity-60 disabled:cursor-not-allowed text-sm"
        >
          {isLoading ? (
            <>
              <div className="w-5 h-5 border-2 border-white/30 border-t-white rounded-full animate-spin" />
              <span>جاري تسجيل الدخول...</span>
            </>
          ) : (
            <>
              <span>تسجيل الدخول</span>
              <ArrowLeft className="w-4 h-4" />
            </>
          )}
        </button>
      </form>

      {/* Quick Demo Credentials helper */}
      <div className="mt-8 pt-6 border-t border-slate-100 text-center">
        <p className="text-xs font-semibold text-slate-400 mb-3 tracking-wide">
          حسابات تجريبية سريعة للبدء:
        </p>
        <div className="grid grid-cols-2 gap-2">
          <button
            type="button"
            onClick={() => fillCredentials('admin@company.com', 'Admin123!')}
            className="px-3 py-2 rounded-lg bg-slate-50 hover:bg-blue-50/80 border border-slate-200/80 text-xs font-medium text-slate-700 hover:text-blue-700 transition-all text-center flex items-center justify-center gap-1.5 cursor-pointer"
          >
            <Shield className="w-3.5 h-3.5 text-blue-600" />
            <span>مدير النظام</span>
          </button>
          <button
            type="button"
            onClick={() => fillCredentials('employee@company.com', 'Employee123!')}
            className="px-3 py-2 rounded-lg bg-slate-50 hover:bg-emerald-50/80 border border-slate-200/80 text-xs font-medium text-slate-700 hover:text-emerald-700 transition-all text-center flex items-center justify-center gap-1.5 cursor-pointer"
          >
            <UserIcon className="w-3.5 h-3.5 text-emerald-600" />
            <span>موظف الدعم</span>
          </button>
        </div>
      </div>
    </div>
  );
};
