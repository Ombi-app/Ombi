// also exported from '@storybook/angular' if you can deal with breaking changes in 6.1
import { APP_BASE_HREF, CommonModule } from '@angular/common';
import { Story, Meta, moduleMetadata } from '@storybook/angular';
import { SharedModule } from '../../../../shared/shared.module';
import { PlexFormComponent } from './plex-form.component';



// More on default export: https://storybook.js.org/docs/angular/writing-stories/introduction#default-export
export default {
  title: 'Plex Form Component',
  component: PlexFormComponent,
  decorators: [
    moduleMetadata({
      providers: [
        {
          provide: APP_BASE_HREF,
          useValue: ""
        },
      ],
      imports: [
        CommonModule,
        SharedModule
      ]
    })
  ]
} as Meta;

// More on component templates: https://storybook.js.org/docs/angular/writing-stories/introduction#using-args
const Template: Story<PlexFormComponent> = (args: PlexFormComponent) => ({
  props: args,
});

export const Default = Template.bind({});
Default.args = {
};

