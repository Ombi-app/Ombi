import { ModuleWithProviders, NgModule } from '@angular/core';
import { HumanizePipe } from './HumanizePipe';
import { TranslateStatusPipe } from './TranslateStatus';
import { ThousandShortPipe } from './ThousandShortPipe';
import { SafePipe } from './SafePipe';
import { QualityPipe } from './QualityPipe';
import { OrderPipe } from './OrderPipe';

@NgModule({
	imports: [],
	declarations: [HumanizePipe, ThousandShortPipe, SafePipe, QualityPipe, TranslateStatusPipe, OrderPipe],
	exports: [HumanizePipe, ThousandShortPipe, SafePipe, QualityPipe, TranslateStatusPipe, OrderPipe],
})
export class PipeModule {
	public static forRoot(): ModuleWithProviders<PipeModule> {
		return {
			ngModule: PipeModule,
			providers: [],
		};
	}
}
