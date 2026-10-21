import { useMemo, useState } from 'react';
import { Compass, RefreshCw } from 'lucide-react';
import type { DashboardOverview } from '../types/api';

type Point = DashboardOverview['geographicDistribution'][number];

/** Egypt geographic bounds used to project lat/lon onto the map canvas. */
const LAT_MIN = 21.7;
const LAT_MAX = 31.7;
const LON_MIN = 24.7;
const LON_MAX = 36.9;

function project(lat: number, lon: number) {
  const x = ((lon - LON_MIN) / (LON_MAX - LON_MIN)) * 100;
  const y = ((LAT_MAX - lat) / (LAT_MAX - LAT_MIN)) * 100;
  return {
    left: `${clamp(x, 2, 98)}%`,
    top: `${clamp(y, 2, 98)}%`,
  };
}

function clamp(n: number, min: number, max: number) {
  return Math.min(max, Math.max(min, n));
}

function radiusFor(count: number) {
  if (count <= 0) return 10;
  if (count <= 10) return 18 + count;
  if (count <= 100) return 28 + count * 0.12;
  return Math.min(78, 42 + Math.sqrt(count) * 1.8);
}

export function EgyptMap({ points }: { points: Point[] }) {
  const [tick, setTick] = useState(0);
  const total = useMemo(() => points.reduce((s, p) => s + p.memberCount, 0), [points]);
  const visible = useMemo(
    () => [...points].sort((a, b) => b.memberCount - a.memberCount),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [points, tick],
  );

  return (
    <div className="egypt-map">
      <div className="egypt-map__canvas">
        <button type="button" className="egypt-compass" aria-label="Compass">
          <Compass size={18} />
        </button>

        <button
          type="button"
          className="egypt-reload"
          onClick={() => setTick((t) => t + 1)}
        >
          <RefreshCw size={14} /> Reload Map
        </button>

        <div className="egypt-map__legend egypt-map__legend--tr">
          <strong>Subscribers</strong>
          <div className="egypt-map__legend-row">
            <i className="dot low" /> Low (1–10)
          </div>
          <div className="egypt-map__legend-row">
            <i className="dot medium" /> Medium (11–100)
          </div>
          <div className="egypt-map__legend-row">
            <i className="dot high" /> High (100+)
          </div>
          <p className="egypt-map__total">{total} total across locations</p>
        </div>

        <svg viewBox="0 0 400 520" className="egypt-map__svg" aria-hidden>
          <defs>
            <linearGradient id="egLand" x1="0" y1="0" x2="0.2" y2="1">
              <stop offset="0%" stopColor="#e8eef5" />
              <stop offset="100%" stopColor="#d3dde9" />
            </linearGradient>
          </defs>
          <path
            fill="url(#egLand)"
            stroke="#c0ccd9"
            strokeWidth="2"
            strokeLinejoin="round"
            d="M90 55
               C115 38 145 30 175 28
               C205 26 235 32 258 45
               C278 56 292 72 298 92
               C304 112 302 130 310 148
               C322 172 340 188 348 212
               C356 238 348 258 330 270
               C314 280 304 296 298 316
               C290 342 282 372 272 402
               C262 430 248 458 230 478
               C210 500 186 508 162 504
               C138 500 122 480 114 452
               C106 422 102 390 98 358
               C94 326 88 294 82 264
               C76 232 70 200 66 170
               C62 140 68 108 78 82
               C82 70 86 60 90 55 Z"
          />
          <path
            fill="#dce5f0"
            stroke="#c0ccd9"
            strokeWidth="1.5"
            d="M298 92 C312 104 330 122 342 142 C350 158 354 176 348 192
               C336 180 322 166 310 148 C304 130 300 110 298 92 Z"
          />
        </svg>

        {visible.map((p) => {
          if (p.memberCount <= 0) return null;
          const pos = project(p.latitude, p.longitude);
          const size = radiusFor(p.memberCount);
          const density = p.densityLevel.toLowerCase();
          return (
            <div key={p.governorateId} className="egypt-marker" style={pos}>
              <span
                className={`egypt-bubble density-${density}`}
                style={{ width: size, height: size }}
              />
              <span className="egypt-label">
                {p.name}: {p.memberCount}
              </span>
            </div>
          );
        })}
      </div>
    </div>
  );
}
