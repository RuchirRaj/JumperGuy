void sdRoundedBox_float( float2 p, float2 b, float4 r, out float d )
{
    r.xy = (p.x>0.0)?r.xy : r.zw;
    r.x  = (p.y>0.0)?r.x  : r.y;
    float2 q = abs(p)-b+r.x;
    d = min(max(q.x,q.y),0.0) + length(max(q,0.0)) - r.x;
}

void sdRoundedBox_half( half2 p, half2 b, half4 r, out half d )
{
    r.xy = (p.x>0.0)?r.xy : r.zw;
    r.x  = (p.y>0.0)?r.x  : r.y;
    half2 q = abs(p)-b+r.x;
    d = min(max(q.x,q.y),0.0) + length(max(q,0.0)) - r.x;
}