import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { EmployeeSupportPage } from '../EmployeeSupportPage';
import supportService from '../../services/supportService';
import { describe, it, expect, vi, beforeEach } from 'vitest';

vi.mock('../../services/supportService', () => ({
  default: {
    submitQuestion: vi.fn(),
  },
}));

describe('EmployeeSupportPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders correctly with Arabic content', () => {
    render(<EmployeeSupportPage />);
    
    expect(screen.getByText('ما المشكلة التي يواجهها العميل؟')).toBeInTheDocument();
    expect(screen.getByPlaceholderText('اكتب تفاصيل المشكلة هنا...')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /إرسال/i })).toBeInTheDocument();
  });

  it('disables submit button when problem input is empty', () => {
    render(<EmployeeSupportPage />);
    
    const submitButton = screen.getByRole('button', { name: /إرسال/i });
    expect(submitButton).toBeDisabled();
    
    const textarea = screen.getByPlaceholderText('اكتب تفاصيل المشكلة هنا...');
    fireEvent.change(textarea, { target: { value: 'مشكلة في النظام' } });
    
    expect(submitButton).not.toBeDisabled();
  });

  it('shows processing state and disables input during submission', async () => {
    let resolveSubmit: any;
    (supportService.submitQuestion as any).mockImplementation(() => {
      return new Promise(resolve => {
        resolveSubmit = resolve;
      });
    });

    render(<EmployeeSupportPage />);
    
    const textarea = screen.getByPlaceholderText('اكتب تفاصيل المشكلة هنا...');
    fireEvent.change(textarea, { target: { value: 'مشكلة' } });
    
    const submitButton = screen.getByRole('button', { name: /إرسال/i });
    fireEvent.click(submitButton);
    
    expect(screen.getByText(/جاري تحليل المشكلة/i)).toBeInTheDocument();
    expect(textarea).toBeDisabled();

    // Resolve the promise to clean up
    resolveSubmit({ status: 'NoAnswer', answered: false });
    await waitFor(() => {
      expect(screen.getByText(/عذراً، لم يتم العثور على حل/i)).toBeInTheDocument();
    });
  });
});
