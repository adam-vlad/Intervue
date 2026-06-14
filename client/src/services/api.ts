import type {
  CvProfileDto,
  CvProfileSummaryDto,
  InterviewDto,
  InterviewSummaryDto,
  ProblemDetails,
} from '../types/api';

const API_BASE = import.meta.env.VITE_API_URL ?? 'http://localhost:5000/api/v1';

/* ── Generic fetch wrapper ─────────────────────────────────────── */

async function request<T>(url: string, options?: RequestInit): Promise<T> {
  const response = await fetch(`${API_BASE}${url}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...options?.headers,
    },
  });

  if (!response.ok) {
    const problem: ProblemDetails = await response.json().catch(() => ({
      title: 'Network error',
      status: response.status,
    }));
    throw problem;
  }

  if (response.status === 204) return undefined as T;
  return response.json();
}

/* ── CV endpoints ──────────────────────────────────────────────── */

export async function uploadCv(file: File): Promise<string> {
  const formData = new FormData();
  formData.append('file', file);

  const response = await fetch(`${API_BASE}/cv/upload`, {
    method: 'POST',
    body: formData,
  });

  if (!response.ok) {
    const problem: ProblemDetails = await response.json().catch(() => ({
      title: 'Upload failed',
      status: response.status,
    }));
    throw problem;
  }

  return response.json();
}

export function parseCv(cvProfileId: string): Promise<CvProfileDto> {
  return request('/cv/parse', {
    method: 'POST',
    body: JSON.stringify({ cvProfileId }),
  });
}

export function getAllCvProfiles(): Promise<CvProfileSummaryDto[]> {
  return request('/cv');
}

export function getCvProfile(id: string): Promise<CvProfileDto> {
  return request(`/cv/${id}`);
}

export function getInterviewsByCv(cvProfileId: string): Promise<InterviewSummaryDto[]> {
  return request(`/cv/${cvProfileId}/interviews`);
}

/* ── Interview endpoints ───────────────────────────────────────── */

export function getAllInterviews(): Promise<InterviewSummaryDto[]> {
  return request('/interview');
}

export function getInterview(id: string): Promise<InterviewDto> {
  return request(`/interview/${id}`);
}

export function startInterview(cvProfileId: string, language = 'en'): Promise<InterviewDto> {
  return request('/interview/start', {
    method: 'POST',
    body: JSON.stringify({ cvProfileId, language }),
  });
}

export function sendMessage(interviewId: string, content: string): Promise<InterviewDto> {
  return request('/interview/message', {
    method: 'POST',
    body: JSON.stringify({ interviewId, content }),
  });
}

export function generateFeedback(interviewId: string): Promise<void> {
  return request('/interview/feedback', {
    method: 'POST',
    body: JSON.stringify({ interviewId }),
  });
}
