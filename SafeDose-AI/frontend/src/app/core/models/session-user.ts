export type UserRole = 'User' | 'Admin' | 'SuperAdmin';

export interface SessionUser {
  userName: string;
  email: string;
  role: UserRole;
  name?: string;
  phone?: string;
  roles?: UserRole[];
}
