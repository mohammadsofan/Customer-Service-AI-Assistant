import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { LoginPage } from '../LoginPage';
import { useAuth } from '../../hooks/useAuth';
import { describe, it, expect, vi, beforeEach } from 'vitest';

vi.mock('../../hooks/useAuth', () => ({
  useAuth: vi.fn(),
}));

vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => vi.fn(),
  };
});

describe('LoginPage', () => {
  const mockLogin = vi.fn();

  beforeEach(() => {
    (useAuth as any).mockReturnValue({
      login: mockLogin,
      isAuthenticated: false,
      isAdmin: false,
    });
  });

  it('renders login form properly in Arabic', () => {
    render(
      <MemoryRouter>
        <LoginPage />
      </MemoryRouter>
    );
    expect(screen.getByRole('heading', { name: /تسجيل الدخول/i })).toBeInTheDocument();
    expect(screen.getByPlaceholderText('أدخل بريدك الإلكتروني')).toBeInTheDocument();
    expect(screen.getByPlaceholderText('أدخل كلمة المرور')).toBeInTheDocument();
  });

  it('shows validation labels for empty fields on submit', async () => {
    render(
      <MemoryRouter>
        <LoginPage />
      </MemoryRouter>
    );

    const submitButton = screen.getByRole('button', { name: /تسجيل الدخول/i });
    const form = submitButton.closest('form');
    if (form) {
      fireEvent.submit(form);
    }

    await waitFor(() => {
      expect(document.querySelector('.bg-red-50')).toBeInTheDocument();
    });
  });

  it('shows Arabic alert for invalid credentials (AUTH-003)', async () => {
    mockLogin.mockRejectedValueOnce({
      response: {
        status: 401,
        data: { message: 'You are not authorized to access this resource.' }
      }
    });

    render(
      <MemoryRouter>
        <LoginPage />
      </MemoryRouter>
    );

    fireEvent.change(screen.getByPlaceholderText('أدخل بريدك الإلكتروني'), {
      target: { value: 'test@example.com' },
    });
    fireEvent.change(screen.getByPlaceholderText('أدخل كلمة المرور'), {
      target: { value: 'WrongPassword123' },
    });

    const submitButton = screen.getByRole('button', { name: /تسجيل الدخول/i });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText('بيانات الاعتماد غير صحيحة. يرجى التأكد من البريد وكلمة المرور.')).toBeInTheDocument();
    });
  });
});
