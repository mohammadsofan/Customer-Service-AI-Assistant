import { render, screen, fireEvent, waitFor, within } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { CreateScenarioPage } from '../CreateScenarioPage';
import knowledgeService from '../../services/knowledgeService';
import { describe, it, expect, vi, beforeEach } from 'vitest';

vi.mock('../../services/knowledgeService', () => ({
  default: {
    getCategories: vi.fn().mockResolvedValue([
      { id: 'cat-1', name: 'الدعم الفني' }
    ]),
    createScenario: vi.fn(),
  },
}));

vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => vi.fn(),
  };
});

describe('CreateScenarioPage - ADM-KB-003', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('blocks submission of Active scenario without resolution steps and displays error modal', async () => {
    render(
      <MemoryRouter>
        <CreateScenarioPage />
      </MemoryRouter>
    );

    await waitFor(() => {
      expect(screen.getByText('الدعم الفني')).toBeInTheDocument();
    });

    // Fill in Name, Description, Category
    fireEvent.change(screen.getByTestId('scenario-name-input'), {
      target: { value: 'سيناريو تجريبي للاختبار' },
    });
    fireEvent.change(screen.getByTestId('scenario-description-input'), {
      target: { value: 'وصف مفصل للسيناريو التجريبي' },
    });
    fireEvent.change(screen.getByTestId('scenario-category-select'), {
      target: { value: 'cat-1' },
    });

    // Ensure status is Active (default is Active)
    // Do NOT add resolution steps!

    // Click submit
    const submitBtn = screen.getByTestId('submit-scenario-btn');
    fireEvent.click(submitBtn);

    // Verify knowledgeService.createScenario was NEVER called
    expect(knowledgeService.createScenario).not.toHaveBeenCalled();

    // Verify Error Dialog Modal is visible
    await waitFor(() => {
      const modal = screen.getByTestId('error-modal');
      expect(modal).toBeInTheDocument();
      expect(within(modal).getByText(/تفعيل السيناريو يتطلب إضافة خطوة حل واحدة على الأقل/i)).toBeInTheDocument();
    });
  });
});
