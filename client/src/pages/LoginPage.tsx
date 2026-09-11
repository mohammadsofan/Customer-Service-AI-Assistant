import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';
import { Button } from '../components/Button';
import { Input } from '../components/Input';
import { Alert } from '../components/Alert';
import { LogIn } from 'lucide-react';

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
      setError(err.response?.data?.message || 'فشل تسجيل الدخول. يرجى التحقق من بيانات الاعتماد الخاصة بك.');
      setIsLoading(false);
    }
  };

  // Listen to isAuthenticated changes for already logged in sessions
  const { isAuthenticated, isAdmin: isUserAdmin, user } = useAuth();
  React.useEffect(() => {
    if (isAuthenticated) {
      const roleLower = (user?.role || '').toLowerCase();
      const isAdmin = isUserAdmin || roleLower === 'admin' || roleLower === 'administrator';
      if (isAdmin) {
        navigate('/admin', { replace: true });
      } else {
        navigate('/support', { replace: true });
      }
    }
  }, [isAuthenticated, isUserAdmin, user, navigate]);

  return (
    <div className="w-full max-w-md space-y-8 bg-white p-8 rounded-xl shadow-lg border border-gray-100" dir="rtl">
      <div className="text-center">
        <div className="mx-auto flex h-12 w-12 items-center justify-center rounded-full bg-blue-100 mb-4">
          <LogIn className="h-6 w-6 text-blue-600" />
        </div>
        <h2 className="text-2xl font-bold tracking-tight text-gray-900">تسجيل الدخول</h2>
        <p className="mt-2 text-sm text-gray-600">
          مرحباً بك في نظام الدعم الذكي للموظفين
        </p>
      </div>

      <form className="mt-8 space-y-6" onSubmit={handleSubmit}>
        {error && <Alert type="error" message={error} />}

        <div className="space-y-4">
          <Input
            label="البريد الإلكتروني"
            type="email"
            required
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="أدخل بريدك الإلكتروني"
            disabled={isLoading}
          />

          <Input
            label="كلمة المرور"
            type="password"
            required
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder="أدخل كلمة المرور"
            disabled={isLoading}
          />
        </div>

        <Button
          type="submit"
          className="w-full"
          isLoading={isLoading}
        >
          {isLoading ? 'جاري تسجيل الدخول...' : 'تسجيل الدخول'}
        </Button>
      </form>
    </div>
  );
};
