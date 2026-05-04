import { useState, type FormEvent } from 'react';
import { useAuth } from '../auth/AuthContext';
import * as tasksApi from '../api/tasks';
import type { TaskResponse, TaskStatus, TaskPriority } from '../api/types';

interface Props {
  task?: TaskResponse;
  onSaved: (task: TaskResponse) => void;
  onClose: () => void;
}

export function TaskForm({ task, onSaved, onClose }: Props) {
  const { auth } = useAuth();
  const [title, setTitle] = useState(task?.title ?? '');
  const [description, setDescription] = useState(task?.description ?? '');
  const [dueDate, setDueDate] = useState(task?.dueDate ?? '');
  const [status, setStatus] = useState<TaskStatus>(task?.status ?? 'Todo');
  const [priority, setPriority] = useState<TaskPriority>(task?.priority ?? 'Medium');
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    if (!auth) return;
    setError(null);
    setLoading(true);

    const payload = {
      title: title.trim(),
      description: description.trim() || undefined,
      dueDate: dueDate || undefined,
      status,
      priority,
    };

    try {
      const saved = task
        ? await tasksApi.updateTask(task.id, payload, auth.token)
        : await tasksApi.createTask(payload, auth.token);
      onSaved(saved);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Save failed');
    } finally {
      setLoading(false);
    }
  }

  function handleOverlayClick(e: React.MouseEvent<HTMLDivElement>) {
    if (e.target === e.currentTarget) onClose();
  }

  return (
    <div className="modal-overlay" onClick={handleOverlayClick}>
      <div className="modal-card">
        <h2>{task ? 'Edit Task' : 'New Task'}</h2>
        {error && <div className="error-banner">{error}</div>}
        <form onSubmit={handleSubmit}>
          <label>
            Title <span className="required">*</span>
            <input
              type="text"
              value={title}
              onChange={e => setTitle(e.target.value)}
              required
              maxLength={200}
              autoFocus
            />
          </label>
          <label>
            Description
            <textarea
              value={description}
              onChange={e => setDescription(e.target.value)}
              maxLength={2000}
              rows={3}
            />
          </label>
          <label>
            Due Date
            <input type="date" value={dueDate} onChange={e => setDueDate(e.target.value)} />
          </label>
          <div className="form-row">
            <label>
              Status
              <select value={status} onChange={e => setStatus(e.target.value as TaskStatus)}>
                <option value="Todo">Todo</option>
                <option value="InProgress">In Progress</option>
                <option value="Done">Done</option>
              </select>
            </label>
            <label>
              Priority
              <select value={priority} onChange={e => setPriority(e.target.value as TaskPriority)}>
                <option value="Low">Low</option>
                <option value="Medium">Medium</option>
                <option value="High">High</option>
              </select>
            </label>
          </div>
          <div className="form-actions">
            <button type="button" className="btn-ghost" onClick={onClose} disabled={loading}>
              Cancel
            </button>
            <button type="submit" className="btn-primary" disabled={loading}>
              {loading ? 'Saving…' : 'Save'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
