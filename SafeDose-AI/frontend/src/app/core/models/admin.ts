// models/admin.model.ts
export interface Admin {
  id: string;
  name: string;
  email: string;
  role: 'super-admin' | 'admin' | 'moderator';
  status: 'active' | 'inactive';
  createdAt: string;
}
