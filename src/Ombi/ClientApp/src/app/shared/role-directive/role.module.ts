import { NgModule } from '@angular/core';
import { RoleDirective } from './role-directive';

@NgModule({
	declarations: [RoleDirective],
	exports: [RoleDirective],
})
export class RoleModule {}
