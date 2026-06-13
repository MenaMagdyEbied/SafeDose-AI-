import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  Edit,
  LucideAngularModule,
  Pill,
  Plus,
  Trash2,
  TriangleAlert,
  Users,
  X,
} from 'lucide-angular';
import { FamilyMember } from '../../core/models';
import { MemberForm } from '../../core/models/member-form';

@Component({
  selector: 'app-family-plan',
  imports: [FormsModule, LucideAngularModule],
  templateUrl: './family-plan.html',
  styleUrl: './family-plan.css',
})
export class FamilyPlan {
  plusIcon = Plus;
  usersIcon = Users;
  editIcon = Edit;
  trashIcon = Trash2;
  xIcon = X;
  pillIcon = Pill;
  alertTriangleIcon = TriangleAlert;

  members: FamilyMember[] = [];
  showModal = false;
  editingId: string | null = null;

  allConditions = ['السكري', 'ارتفاع ضغط الدم', 'الربو', 'أمراض القلب', 'الحساسية', 'أخرى'];
  relations = ['زوج/زوجة', 'ابن/ابنة', 'أب/أم', 'أخ/أخت', 'جد/جدة', 'أخرى'];

  form: MemberForm = this.emptyForm();

  ngOnInit(): void {
    const saved = localStorage.getItem('familyMembers');
    if (saved) {
      this.members = JSON.parse(saved);
    } else {
      this.members = this.getMockMembers();
    }
  }

  private getMockMembers(): FamilyMember[] {
    return [
      {
        id: 'm1',
        name: 'سارة محمود',
        age: 36,
        relationship: 'زوجة',
        conditions: ['السكري', 'ارتفاع ضغط الدم'],
        medications: ['ميتفورمين', 'أملوديبين'],
        allergies: 'لا يوجد حساسية',
      },
      {
        id: 'm2',
        name: 'علي محمود',
        age: 10,
        relationship: 'ابن',
        conditions: ['الربو'],
        medications: ['سالمتيرول'],
        allergies: 'الحساسية من الاسبرين',
      },
    ];
  }

  openAddModal(): void {
    this.editingId = null;
    this.form = this.emptyForm();
    this.showModal = true;
  }

  editMember(member: FamilyMember): void {
    this.editingId = member.id;
    this.form = {
      fullName: member.name,
      age: member.age,
      relation: member.relationship,
      conditions: [...member.conditions],
      medsText: member.medications.join('\n'),
      allergies: member.allergies,
    };
    this.showModal = true;
  }

  saveMember(): void {
    if (!this.form.fullName.trim()) return;

    const meds = (this.form.medsText || '')
      .split('\n')
      .map((m: string) => m.trim())
      .filter((m: string) => m.length > 0);

    if (this.editingId) {
      this.members = this.members.map((m) =>
        m.id === this.editingId
          ? {
              ...m,
              name: this.form.fullName,
              age: this.form.age ?? m.age,
              relationship: this.form.relation,
              conditions: [...this.form.conditions],
              medications: meds,
              allergies: this.form.allergies,
            }
          : m,
      );
    } else {
      this.members.push({
        id: Date.now().toString(),
        name: this.form.fullName,
        age: this.form.age ?? 0,
        relationship: this.form.relation,
        conditions: [...this.form.conditions],
        medications: meds,
        allergies: this.form.allergies,
      });
    }

    this.saveToStorage();
    this.closeModal();
  }

  deleteMember(id: string): void {
    this.members = this.members.filter((m) => m.id !== id);
    this.saveToStorage();
  }

  toggleFormCondition(cond: string): void {
    const idx = this.form.conditions.indexOf(cond);
    if (idx === -1) this.form.conditions.push(cond);
    else this.form.conditions.splice(idx, 1);
  }

  closeModal(): void {
    this.showModal = false;
    this.editingId = null;
  }

  private saveToStorage(): void {
    localStorage.setItem('familyMembers', JSON.stringify(this.members));
  }

  private emptyForm(): MemberForm {
    return {
      fullName: '',
      age: null,
      relation: '',
      conditions: [],
      medsText: '',
      allergies: '',
    };
  }
}
