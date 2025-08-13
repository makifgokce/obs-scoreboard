import { Routes } from '@angular/router';
import { LeaderBoard } from './components/leader-board/leader-board';
import { ControlPanel } from './components/control-panel/control-panel';
import { TeamScore } from './components/team-score/team-score';
import { Counter } from './components/counter/counter';

export const routes: Routes = [
    {
        path: '',
        component: ControlPanel
    },
    {
        path: 'leaderboard',
        component: LeaderBoard
    },
    {
        path: 'teamscore',
        component: TeamScore
    },
    {
        path: 'counter',
        component: Counter
    },
    {
        path: 'controlpanel',
        component: ControlPanel
    },
    {
        path: '**',
        component: ControlPanel
    }
];
