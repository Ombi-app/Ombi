// also exported from '@storybook/angular' if you can deal with breaking changes in 6.1
import { APP_BASE_HREF, CommonModule } from '@angular/common';
import { Story, Meta, moduleMetadata } from '@storybook/angular';
import { Observable, of } from 'rxjs';
import { IPlexWatchlistUsers, WatchlistSyncStatus } from '../../../../interfaces';
import { PlexService } from '../../../../services';
import { SharedModule } from '../../../../shared/shared.module';
import { PlexWatchlistComponent } from './plex-watchlist.component';


const mockUsers: IPlexWatchlistUsers[] = 
[
    {
        userName: "Success User",
        userId: "a",
        syncStatus: WatchlistSyncStatus.Successful
    },
    {
        userName: "Failed User",
        userId: "2",
        syncStatus: WatchlistSyncStatus.Failed
    },
    {
        userName: "Not Enabled",
        userId: "2",
        syncStatus: WatchlistSyncStatus.NotEnabled
    },
];

function plexServiveMock(): Partial<PlexService> {
	return {
         getWatchlistUsers: () : Observable<IPlexWatchlistUsers[]> => of(mockUsers),
	};
}

// More on default export: https://storybook.js.org/docs/angular/writing-stories/introduction#default-export
export default {
  title: 'Plex Watchlist Component',
  component: PlexWatchlistComponent,
  decorators: [
    moduleMetadata({
      providers: [
        {
          provide: APP_BASE_HREF,
          useValue: ""
        },
        {
            provide: PlexService,
            useValue: plexServiveMock()
        }
      ],
      imports: [
        CommonModule,
        SharedModule
      ]
    })
  ]
} as Meta;

// More on component templates: https://storybook.js.org/docs/angular/writing-stories/introduction#using-args
const Template: Story<PlexWatchlistComponent> = (args: PlexWatchlistComponent) => ({
  props: args,
});

export const Default = Template.bind({});
Default.args = {
};

