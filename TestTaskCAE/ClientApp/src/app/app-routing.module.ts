import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { ManageComponent } from './home/controls/manage/manage.component';
import { ReportComponent } from './home/controls/report/report.component';

const routes: Routes = [
  { path: 'manage', component: ManageComponent },
  { path: 'report', component: ReportComponent }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
