/* TypeScript interfaces — mirror the backend DTOs exactly */

export type DifficultyLevel = 'Junior' | 'Mid' | 'Senior';
export type InterviewStatus = 'NotStarted' | 'InProgress' | 'Completed';
export type MessageRole = 'Interviewer' | 'Candidate';

/* ── CV Profile ────────────────────────────────────────────────── */

export interface CvProfileDto {
  id: string;
  rawText: string;
  difficultyLevel: DifficultyLevel;
  education: string | null;
  createdAt: string;
  technologies: TechnologyDto[];
  experiences: ExperienceDto[];
  projects: ProjectDto[];
}

export interface TechnologyDto {
  name: string;
  yearsOfExperience: number;
}

export interface ExperienceDto {
  role: string;
  company: string;
  durationMonths: number;
  description: string | null;
}

export interface ProjectDto {
  name: string;
  description: string | null;
  technologiesUsed: string[];
}

export interface CvProfileSummaryDto {
  id: string;
  difficultyLevel: DifficultyLevel;
  education: string | null;
  createdAt: string;
  technologies: string[];
}

/* ── Interview ─────────────────────────────────────────────────── */

export interface InterviewDto {
  id: string;
  cvProfileId: string;
  status: InterviewStatus;
  promptProfile: string;
  startedAt: string;
  completedAt: string | null;
  messages: InterviewMessageDto[];
  feedbackReport: FeedbackReportDto | null;
}

export interface InterviewMessageDto {
  id: string;
  role: MessageRole;
  content: string;
  sentAt: string;
}

export interface InterviewSummaryDto {
  id: string;
  cvProfileId: string;
  status: InterviewStatus;
  startedAt: string;
  completedAt: string | null;
  messageCount: number;
  hasFeedback: boolean;
  overallScore: number | null;
}

/* ── Feedback ──────────────────────────────────────────────────── */

export interface FeedbackReportDto {
  id: string;
  overallScore: number;
  categoryScores: InterviewScoreDto[];
  strengths: string;
  weaknesses: string;
  suggestions: string;
  generatedAt: string;
}

export interface InterviewScoreDto {
  category: string;
  score: number;
}

/* ── API Error (ProblemDetails) ────────────────────────────────── */

export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  errors?: Record<string, string[]>;
}
