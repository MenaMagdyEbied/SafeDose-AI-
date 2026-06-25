import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Check, LucideAngularModule, Pencil, Plus, Shield, Trash2, User, X } from 'lucide-angular';
import { Admin } from '../../../core/models/admin';
import { AdminManagement } from '../services/admin-management';

interface AdminFormState {
  name: string;
  email: string;
  role: Admin['role'];
  password: string;
  newPassword: string;
}

@Component({
  selector: 'app-admin-manager',
  imports: [LucideAngularModule, FormsModule],
  templateUrl: './admin-manager.html',
  styleUrl: './admin-manager.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminManager implements OnInit {
  private readonly adminService = inject(AdminManagement);

  plusIcon = Plus;
  editIcon = Pencil;
  trashIcon = Trash2;
  xIcon = X;
  checkIcon = Check;
  shieldIcon = Shield;
  userIcon = User;

  admins: Admin[] = [];
  isLoading = false;
  isSaving = false;
  isDeleting = false;
  isTogglingStatus = false;

  showForm = false;
  isEditing = false;
  showDeleteDialog = false;
  selectedAdmin: Admin | null = null;

  form: AdminFormState = {
    name: '',
    email: '',
    role: 'admin',
    password: '',
    newPassword: '',
  };

  roles = [
    { value: 'super-admin', label: 'سوبر أدمن' },
    { value: 'admin', label: 'أدمن' },
  ];

  ngOnInit() {
    this.loadAdmins();
  }

  get hasSuperAdmin() {
    return this.admins.some((admin) => admin.role === 'super-admin');
  }

  get availableRoles() {
    return this.hasSuperAdmin
      ? this.roles.filter((role) => role.value !== 'super-admin')
      : this.roles;
  }

  canEdit(admin: Admin) {
    return admin.role !== 'super-admin';
  }

  canDelete(admin: Admin) {
    return admin.role !== 'super-admin';
  }

  loadAdmins() {
    this.isLoading = true;
    this.adminService.getAdmins().subscribe({
      next: (response) => {
        const rawAdmins = Array.isArray(response.items)
          ? response.items
          : Array.isArray(response.data)
            ? response.data
            : [];
        this.admins = rawAdmins.map((admin) => this.mapAdmin(admin));
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      },
    });
  }

  openAdd() {
    this.isEditing = false;
    this.selectedAdmin = null;
    this.form = { name: '', email: '', role: 'admin', password: '', newPassword: '' };
    this.showForm = true;
  }

  openEdit(admin: Admin) {
    if (admin.role === 'super-admin') {
      return;
    }
    this.isEditing = true;
    this.selectedAdmin = admin;
    this.form = {
      name: admin.name,
      email: admin.email,
      role: admin.role,
      password: '',
      newPassword: '',
    };
    this.showForm = true;
  }

  save() {
    if (!this.form.name || !this.form.email) {
      return;
    }

    if (!this.isEditing && !this.form.password) {
      return;
    }

    const payload: Record<string, unknown> = {
      name: this.form.name,
      email: this.form.email,
      role: this.isEditing ? this.form.role : 'admin',
    };

    if (!this.isEditing) {
      payload['password'] = this.form.password;
    } else if (this.form.newPassword) {
      payload['newPassword'] = this.form.newPassword;
    }

    this.isSaving = true;

    const request =
      this.isEditing && this.selectedAdmin
        ? this.adminService.updateAdmin(this.selectedAdmin.id, payload)
        : this.adminService.createAdmin(payload);

    request.subscribe({
      next: () => {
        this.isSaving = false;
        this.showForm = false;
        this.loadAdmins();
      },
      error: () => {
        this.isSaving = false;
      },
    });
  }

  confirmDelete(admin: Admin) {
    if (admin.role === 'super-admin') {
      return;
    }
    this.selectedAdmin = admin;
    this.showDeleteDialog = true;
  }

  executeDelete() {
    if (!this.selectedAdmin) {
      return;
    }

    this.isDeleting = true;
    this.adminService.deleteAdmin(this.selectedAdmin.id).subscribe({
      next: () => {
        this.admins = this.admins.filter((admin) => admin.id !== this.selectedAdmin?.id);
        this.showDeleteDialog = false;
        this.selectedAdmin = null;
        this.isDeleting = false;
      },
      error: () => {
        this.isDeleting = false;
      },
    });
  }

  toggleStatus(admin: Admin) {
    this.isTogglingStatus = true;
    this.adminService.toggleAdminStatus(admin.id, admin.status !== 'active').subscribe({
      next: () => {
        this.admins = this.admins.map((item) =>
          item.id === admin.id
            ? { ...item, status: admin.status === 'active' ? 'inactive' : 'active' }
            : item,
        );
        this.isTogglingStatus = false;
      },
      error: () => {
        this.isTogglingStatus = false;
      },
    });
  }

  getRoleLabel(role: string) {
    return this.roles.find((item) => item.value === role)?.label ?? role;
  }

  getStatusLabel(status: string) {
    return status === 'active' ? 'نشط' : 'غير نشط';
  }

  private mapAdmin(admin: unknown): Admin {
    const source = admin as Record<string, unknown>;
    const role = String(source['role'] ?? 'admin');
    const status =
      source['isActive'] === false || source['status'] === 'inactive' ? 'inactive' : 'active';

    return {
      id: String(source['id'] ?? ''),
      name: String(source['name'] ?? ''),
      email: String(source['email'] ?? ''),
      role:
        role === 'super-admin' || role === 'admin' || role === 'moderator'
          ? (role as Admin['role'])
          : 'admin',
      status: status as Admin['status'],
      createdAt: String(source['createdAt'] ?? source['created_at'] ?? ''),
    };
  }
}
