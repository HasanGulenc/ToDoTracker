import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import { TaskForm } from './TaskForm';
import type { TaskResponse } from '../api/types';

const mockCreateTask = vi.fn();
const mockUpdateTask = vi.fn();

vi.mock('../auth/AuthContext', () => ({
  useAuth: () => ({ auth: { token: 'test-token', email: 'user@test.com' }, login: vi.fn(), register: vi.fn(), signOut: vi.fn() }),
}));

vi.mock('../api/tasks', () => ({
  createTask: (...args: unknown[]) => mockCreateTask(...args),
  updateTask: (...args: unknown[]) => mockUpdateTask(...args),
}));

const mockTask: TaskResponse = {
  id: 1,
  title: 'Existing task',
  description: 'Some description',
  dueDate: null,
  status: 'Todo',
  priority: 'Medium',
  createdAt: '2026-05-01T00:00:00Z',
  updatedAt: '2026-05-01T00:00:00Z',
};

describe('TaskForm', () => {
  const onSaved = vi.fn();
  const onClose = vi.fn();

  beforeEach(() => {
    mockCreateTask.mockReset();
    mockUpdateTask.mockReset();
    onSaved.mockReset();
    onClose.mockReset();
  });

  it('renders create form with empty fields', () => {
    render(<TaskForm onSaved={onSaved} onClose={onClose} />);
    expect(screen.getByRole('heading', { name: /new task/i })).toBeInTheDocument();
    expect(screen.getByLabelText(/title/i)).toHaveValue('');
  });

  it('renders edit form pre-filled with task data', () => {
    render(<TaskForm task={mockTask} onSaved={onSaved} onClose={onClose} />);
    expect(screen.getByRole('heading', { name: /edit task/i })).toBeInTheDocument();
    expect(screen.getByLabelText(/title/i)).toHaveValue('Existing task');
  });

  it('calls onSaved after successful create', async () => {
    const saved = { ...mockTask, id: 2, title: 'New task' };
    mockCreateTask.mockResolvedValue(saved);

    render(<TaskForm onSaved={onSaved} onClose={onClose} />);
    await userEvent.type(screen.getByLabelText(/title/i), 'New task');
    await userEvent.click(screen.getByRole('button', { name: /^save$/i }));

    await waitFor(() => expect(onSaved).toHaveBeenCalledWith(saved));
  });

  it('shows error and keeps form open on server failure', async () => {
    mockCreateTask.mockRejectedValue(new Error('Title already exists.'));

    render(<TaskForm onSaved={onSaved} onClose={onClose} />);
    await userEvent.type(screen.getByLabelText(/title/i), 'Duplicate task');
    await userEvent.click(screen.getByRole('button', { name: /^save$/i }));

    await waitFor(() =>
      expect(screen.getByText('Title already exists.')).toBeInTheDocument()
    );
    expect(onSaved).not.toHaveBeenCalled();
    expect(screen.getByLabelText(/title/i)).toBeInTheDocument();
  });
});
