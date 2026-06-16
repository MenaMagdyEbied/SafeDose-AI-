import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule, Plus, Pencil, Trash2, X, Check, Shield, User } from 'lucide-angular';
import { Admin } from '../../../core/models/admin';

@Component({
  selector: 'app-admin-manager',
  imports: [LucideAngularModule, FormsModule],
  templateUrl: './admin-manager.html',
  styleUrl: './admin-manager.css',
})
export class AdminManager {
  plusIcon = Plus;
  editIcon = Pencil;
  trashIcon = Trash2;
  xIcon = X;
  checkIcon = Check;
  shieldIcon = Shield;
  userIcon = User;

  admins: Admin[] = [
    {
      id: '1',
      name: 'أحمد محمد',
      email: 'ahmed@safedose.ai',
      role: 'super-admin',
      status: 'active',
      createdAt: '2024-01-01',
    },
    {
      id: '2',
      name: 'سارة علي',
      email: 'sara@safedose.ai',
      role: 'admin',
      status: 'active',
      createdAt: '2024-02-15',
    },
    {
      id: '3',
      name: 'محمود حسن',
      email: 'mahmoud@safedose.ai',
      role: 'moderator',
      status: 'inactive',
      createdAt: '2024-03-10',
    },
  ];

  showForm = false;
  isEditing = false;
  showDeleteDialog = false;
  selectedAdmin: Admin | null = null;

  form: Omit<Admin, 'id' | 'createdAt'> = {
    name: '',
    email: '',
    role: 'admin',
    status: 'active',
  };

  roles = [
    { value: 'super-admin', label: 'سوبر أدمن' },
    { value: 'admin', label: 'أدمن' },
    { value: 'moderator', label: 'مشرف' },
  ];

  openAdd() {
    this.isEditing = false;
    this.form = { name: '', email: '', role: 'admin', status: 'active' };
    this.showForm = true;
  }

  openEdit(admin: Admin) {
    this.isEditing = true;
    this.selectedAdmin = admin;
    this.form = { name: admin.name, email: admin.email, role: admin.role, status: admin.status };
    this.showForm = true;
  }

  save() {
    if (this.isEditing && this.selectedAdmin) {
      const index = this.admins.findIndex((a) => a.id === this.selectedAdmin!.id);
      this.admins[index] = { ...this.selectedAdmin, ...this.form };
    } else {
      this.admins.push({
        ...this.form,
        id: Date.now().toString(),
        createdAt: new Date().toISOString().split('T')[0],
      });
    }
    this.showForm = false;
  }

  confirmDelete(admin: Admin) {
    this.selectedAdmin = admin;
    this.showDeleteDialog = true;
  }

  executeDelete() {
    this.admins = this.admins.filter((a) => a.id !== this.selectedAdmin!.id);
    this.showDeleteDialog = false;
    this.selectedAdmin = null;
  }

  getRoleLabel(role: string) {
    return this.roles.find((r) => r.value === role)?.label ?? role;
  }
}
